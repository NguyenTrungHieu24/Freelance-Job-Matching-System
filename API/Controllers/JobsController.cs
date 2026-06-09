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

        // POST: api/jobs
        [HttpPost]
        [Authorize(Roles = "EMPLOYER")]
        public async Task<ActionResult<JobDTO>> CreateJob([FromBody] CreateJobDto dto)
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

            var resultDto = _mapper.Map<JobDTO>(createdJob);

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
            if (!Enum.TryParse<JobStatus>(dto.Status.Trim(), true, out var parsedStatus) || parsedStatus == JobStatus.DELETED)
            {
                return BadRequest(new { message = "Invalid job status value" });
            }

            // Update Job fields
            job.Title = dto.Title.Trim();
            job.Description = dto.Description.Trim();
            job.Budget = dto.Budget;
            job.CategoryId = dto.CategoryId;
            job.Deadline = dto.Deadline;
            job.Status = parsedStatus;

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

        [HttpGet]
        public async Task<IActionResult> GetJobs([FromQuery] FilterJobDTO filter) {
            var query = _context.Jobs
                .Include(x => x.Category)
                .Include(x => x.EmployerProfile)
                .ThenInclude(x => x.Account)
                .AsQueryable();

            query = ApplyPermission(query);

            #region Filters

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                query = query.Where(x =>
                    x.Title.Contains(filter.Keyword) ||
                    x.Description.Contains(filter.Keyword));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
            }

            if (filter.EmployerProfileId.HasValue)
            {
                query = query.Where(x => x.EmployerProfileId == filter.EmployerProfileId.Value);
            }

            if (filter.MinBudget.HasValue)
            {
                query = query.Where(x => x.Budget >= filter.MinBudget.Value);
            }

            if (filter.MaxBudget.HasValue)
            {
                query = query.Where(x => x.Budget <= filter.MaxBudget.Value);
            }

            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= filter.CreatedTo.Value);
            }

            if (filter.DeadlineFrom.HasValue)
            {
                query = query.Where(x => x.Deadline >= filter.DeadlineFrom.Value);
            }

            if (filter.DeadlineTo.HasValue)
            {
                query = query.Where(x => x.Deadline <= filter.DeadlineTo.Value);
            }

            if (filter.Temperature.HasValue)
            {
                switch (filter.Temperature.Value)
                {
                    case JobTemperature.Cool:
                        query = query.Where(x => x.Applications.Count < 2);
                        break;

                    case JobTemperature.Warm:
                        query = query.Where(x => x.Applications.Count >= 2 && x.Applications.Count < 8);
                        break;

                    case JobTemperature.Hot:
                        query = query.Where(x => x.Applications.Count >= 8);
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.EmployerKeyword))
            {
                query = query.Where(x => x.EmployerProfile.Account.FullName.Contains(filter.EmployerKeyword));
            }

            if (filter.SkillIds.Count > 0)
            {
                query = query.Where(x => x.JobSkills.Any(js => filter.SkillIds.Contains(js.SkillId)));
            }

            #endregion

            #region Sorting

            query = filter.SortBy?.ToLower() switch
            {
                "title" => filter.IsDescending
                    ? query.OrderByDescending(x => x.Title)
                    : query.OrderBy(x => x.Title),

                "budget" => filter.IsDescending
                    ? query.OrderByDescending(x => x.Budget)
                    : query.OrderBy(x => x.Budget),

                "deadline" => filter.IsDescending
                    ? query.OrderByDescending(x => x.Deadline)
                    : query.OrderBy(x => x.Deadline),

                _ => filter.IsDescending
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt)
            };

            #endregion

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ProjectTo<JobDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var result = new PaginateResult<JobDTO>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var query = _context.Jobs
                .Include(x => x.Category)
                .Include(x => x.EmployerProfile)
                    .ThenInclude(x => x.Account)
                .Include(x => x.JobSkills)
                    .ThenInclude(x => x.Skill)
                .Include(x => x.Applications)
                .AsQueryable();

            query = ApplyPermission(query);

            var job = await query.FirstOrDefaultAsync(x => x.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<JobDTO>(job));
        }

        [HttpPost("{id:int}/apply")]
        [Authorize(Roles = "FREELANCER")]
        public async Task<IActionResult> ApplyJob(int id, [FromBody] ApplyJobDto dto)
        {
            var userId = _user.UserId;

            var freelancer = await _context.FreelancerProfiles
                .FirstOrDefaultAsync(f => f.AccountId == userId);

            if (freelancer == null)
            {
                return BadRequest(new { message = "Freelancer profile not found." });
            }

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.Status == JobStatus.ACTIVE);
            if (job == null)
            {
                return NotFound(new { message = "Job not found or not active." });
            }

            var alreadyApplied = await _context.Applications
                .AnyAsync(a => a.JobId == id && a.FreelancerProfileId == freelancer.Id);

            if (alreadyApplied)
            {
                return BadRequest(new { message = "You have already applied for this job." });
            }

            var application = new Application
            {
                JobId = id,
                FreelancerProfileId = freelancer.Id,
                CoverLetter = dto.CoverLetter?.Trim() ?? string.Empty,
                Status = ApplicationStatus.PENDING,
                AppliedAt = DateTime.Now
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Applied successfully!" });
        }

        private IQueryable<Job> ApplyPermission(IQueryable<Job> query)
        {
            switch (_user.Role.ToLower())
            {
                case "admin":
                    return query;

                case "employer":
                    return query.Where(x => x.EmployerProfile.AccountId == _user.UserId);

                case "freelancer":
                    return query.Where(x => x.Status == JobStatus.ACTIVE);

                default:
                    return query.Where(x => x.Status == JobStatus.ACTIVE);
            }
        }
    }
}
