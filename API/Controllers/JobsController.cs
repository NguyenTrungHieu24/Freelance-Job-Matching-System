using API.Services.Auth;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects;
using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobsController : BaseController
    {
        public JobsController(AppDbContext context, IMapper mapper, IUserService user) 
            : base(context, mapper, user)
        {
        }

        // GET: api/jobs
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginateResult<JobDto>>> GetJobs([FromQuery] FilterJobDto q)
        {
            // Exclude soft-deleted jobs
            var query = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.EmployerProfile)
                    .ThenInclude(e => e.Account)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Include(j => j.Applications)
                .Where(j => j.Status != JobStatus.DELETED)
                .AsQueryable();

            // Keyword filter (title or description)
            if (!string.IsNullOrWhiteSpace(q.Keyword))
            {
                var keyword = q.Keyword.Trim().ToLower();
                query = query.Where(j => j.Title.ToLower().Contains(keyword) || 
                                         j.Description.ToLower().Contains(keyword));
            }

            // Category filter
            if (q.CategoryId.HasValue)
            {
                query = query.Where(j => j.CategoryId == q.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(q.EmployerKeyword))
            {
                var employerKeyword = q.EmployerKeyword.Trim().ToLower();
                query = query.Where(j =>
                    j.EmployerProfile.CompanyName.ToLower().Contains(employerKeyword) ||
                    j.EmployerProfile.Account.FullName.ToLower().Contains(employerKeyword));
            }

            if (q.EmployerProfileId.HasValue)
            {
                query = query.Where(j => j.EmployerProfileId == q.EmployerProfileId.Value);
            }

            // Skill filter
            if (q.SkillId.HasValue)
            {
                query = query.Where(j => j.JobSkills.Any(js => js.SkillId == q.SkillId.Value));
            }

            if (q.SkillIds.Count > 0)
            {
                query = query.Where(j => j.JobSkills.Any(js => q.SkillIds.Contains(js.SkillId)));
            }

            // Budget filter
            if (q.MinBudget.HasValue)
            {
                query = query.Where(j => j.Budget >= q.MinBudget.Value);
            }
            if (q.MaxBudget.HasValue)
            {
                query = query.Where(j => j.Budget <= q.MaxBudget.Value);
            }

            if (q.CreatedFrom.HasValue)
            {
                query = query.Where(j => j.CreatedAt.Date >= q.CreatedFrom.Value.Date);
            }

            if (q.CreatedTo.HasValue)
            {
                query = query.Where(j => j.CreatedAt.Date <= q.CreatedTo.Value.Date);
            }

            if (q.DeadlineFrom.HasValue)
            {
                query = query.Where(j => j.Deadline.HasValue && j.Deadline.Value.Date >= q.DeadlineFrom.Value.Date);
            }

            if (q.DeadlineTo.HasValue)
            {
                query = query.Where(j => j.Deadline.HasValue && j.Deadline.Value.Date <= q.DeadlineTo.Value.Date);
            }

            if (q.Temperature.HasValue)
            {
                query = q.Temperature.Value switch
                {
                    JobTemperature.Hot => query.Where(j => j.Applications.Count >= 10),
                    JobTemperature.Warm => query.Where(j => j.Applications.Count >= 5 && j.Applications.Count < 10),
                    JobTemperature.Cool => query.Where(j => j.Applications.Count < 5),
                    _ => query
                };
            }

            if (q.Status.HasValue)
            {
                if (q.Status.Value != JobStatus.DELETED)
                    query = query.Where(j => j.Status == q.Status.Value);
            }
            else
            {
                // Default to ACTIVE if no status is specified
                query = query.Where(j => j.Status == JobStatus.ACTIVE);
            }

            var totalItems = await query.CountAsync();

            query = (q.SortBy?.Trim().ToLower()) switch
            {
                "title" => q.IsDescending ? query.OrderByDescending(j => j.Title) : query.OrderBy(j => j.Title),
                "budget" => q.IsDescending ? query.OrderByDescending(j => j.Budget) : query.OrderBy(j => j.Budget),
                "deadline" => q.IsDescending ? query.OrderByDescending(j => j.Deadline) : query.OrderBy(j => j.Deadline),
                "applications" => q.IsDescending ? query.OrderByDescending(j => j.Applications.Count) : query.OrderBy(j => j.Applications.Count),
                _ => q.IsDescending ? query.OrderByDescending(j => j.CreatedAt) : query.OrderBy(j => j.CreatedAt)
            };

            var page = q.Page <= 0 ? 1 : q.Page;
            var pageSize = q.PageSize <= 0 ? 10 : q.PageSize;

            var jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mappedJobs = _mapper.Map<List<JobDto>>(jobs);

            return Ok(new PaginateResult<JobDto>
            {
                Items = mappedJobs,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems
            });
        }

        // GET: api/jobs/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<JobDto>> GetJob(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.EmployerProfile)
                    .ThenInclude(e => e.Account)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.Id == id && j.Status != JobStatus.DELETED);

            if (job == null)
            {
                return NotFound(new { message = "Job not found" });
            }

            var dto = _mapper.Map<JobDto>(job);
            return Ok(dto);
        }

        // POST: api/jobs
        [HttpPost]
        [Authorize(Roles = "EMPLOYER")]
        public async Task<ActionResult<JobDto>> CreateJob([FromBody] CreateJobDto dto)
        {
            var userId = _user.UserId;

            // Resolve Employer Profile
            var employerProfile = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.AccountId == userId);

            if (employerProfile == null)
            {
                return BadRequest(new { message = "Employer profile not found for this user" });
            }

            // Verify category exists
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { message = "Invalid CategoryId" });
            }

            // Map CreateJobDto to Job entity
            var job = new Job
            {
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                Budget = dto.Budget,
                CategoryId = dto.CategoryId,
                Deadline = dto.Deadline,
                Status = JobStatus.ACTIVE,
                EmployerProfileId = employerProfile.Id,
                CreatedAt = DateTime.Now
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync(); // Saves job and generates its Id

            // Add JobSkills if provided
            if (dto.Skills != null && dto.Skills.Any())
            {
                // Filter out non-existent skills
                var validSkillIds = await _context.Skills
                    .Where(s => dto.Skills.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync();

                foreach (var skillId in validSkillIds)
                {
                    _context.JobSkills.Add(new JobSkill
                    {
                        JobId = job.Id,
                        SkillId = skillId
                    });
                }

                await _context.SaveChangesAsync();
            }

            // Fetch the fully populated job to return
            var createdJob = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.EmployerProfile)
                    .ThenInclude(e => e.Account)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == job.Id);

            var resultDto = _mapper.Map<JobDto>(createdJob);

            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, resultDto);
        }

        // PUT: api/jobs/5
        [HttpPut("{id}")]
        [Authorize(Roles = "EMPLOYER")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobDto dto)
        {
            var userId = _user.UserId;

            // Find existing Job
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id && j.Status != JobStatus.DELETED);

            if (job == null)
            {
                return NotFound(new { message = "Job not found" });
            }

            // Resolve Employer Profile and verify ownership
            var employerProfile = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.AccountId == userId);

            if (employerProfile == null || job.EmployerProfileId != employerProfile.Id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have permission to modify this job posting" });
            }

            // Verify category exists
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { message = "Invalid CategoryId" });
            }

            // Parse and validate Status
            if (!Enum.TryParse<JobStatus>(dto.Status.Trim(), true, out var newStatus) || newStatus == JobStatus.DELETED)
            {
                return BadRequest(new { message = "Invalid job status value" });
            }

            // Update Job fields
            job.Title = dto.Title.Trim();
            job.Description = dto.Description.Trim();
            job.Budget = dto.Budget;
            job.CategoryId = dto.CategoryId;
            job.Deadline = dto.Deadline;
            job.Status = newStatus;

            // Update Skills
            var existingSkills = _context.JobSkills.Where(js => js.JobId == job.Id);
            _context.JobSkills.RemoveRange(existingSkills);

            if (dto.Skills != null && dto.Skills.Any())
            {
                // Filter out non-existent skills
                var validSkillIds = await _context.Skills
                    .Where(s => dto.Skills.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync();

                foreach (var skillId in validSkillIds)
                {
                    _context.JobSkills.Add(new JobSkill
                    {
                        JobId = job.Id,
                        SkillId = skillId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Job updated successfully" });
        }

        // DELETE: api/jobs/5 (Soft Delete)
        [HttpDelete("{id}")]
        [Authorize(Roles = "EMPLOYER")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = _user.UserId;

            // Find existing Job
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id && j.Status != JobStatus.DELETED);

            if (job == null)
            {
                return NotFound(new { message = "Job not found" });
            }

            // Resolve Employer Profile and verify ownership
            var employerProfile = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.AccountId == userId);

            if (employerProfile == null || job.EmployerProfileId != employerProfile.Id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have permission to delete this job posting" });
            }

            // Perform Soft Delete
            job.Status = JobStatus.DELETED;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Job deleted successfully" });
        }

    }
}
