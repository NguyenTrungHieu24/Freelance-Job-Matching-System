using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

//Tao controller xu ly HTTP Request
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
            // Exclude soft-deleted jobs (Status = "DELETED")
            var deletedStatus = JobStatus.DELETED.ToString();
            var query = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.EmployerProfile)
                    .ThenInclude(e => e.Account)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Where(j => j.Status != deletedStatus)
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

            // Skill filter
            if (q.SkillId.HasValue)
            {
                query = query.Where(j => j.JobSkills.Any(js => js.SkillId == q.SkillId.Value));
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

            // Status filter (e.g. ACTIVE or CLOSED)
            if (!string.IsNullOrWhiteSpace(q.Status))
            {
                var statusStr = q.Status.Trim().ToUpper();
                // Ensure they don't query deleted ones through here
                if (statusStr != deletedStatus)
                {
                    query = query.Where(j => j.Status == statusStr);
                }
            }
            else
            {
                // Default to ACTIVE if no status is specified
                var activeStatus = JobStatus.ACTIVE.ToString();
                query = query.Where(j => j.Status == activeStatus);
            }

            var totalItems = await query.CountAsync();

            // Order by most recent job first
            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .ToListAsync();

            var mappedJobs = _mapper.Map<List<JobDto>>(jobs);

            return Ok(new PaginateResult<JobDto>
            {
                Items = mappedJobs,
                PageNumber = q.Page,
                PageSize = q.PageSize,
                TotalItems = totalItems
            });
        }

        // GET: api/jobs/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<JobDto>> GetJob(int id)
        {
            var deletedStatus = JobStatus.DELETED.ToString();
            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.EmployerProfile)
                    .ThenInclude(e => e.Account)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == id && j.Status != deletedStatus);

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
                Status = JobStatus.ACTIVE.ToString(),
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
            var deletedStatus = JobStatus.DELETED.ToString();

            // Find existing Job
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id && j.Status != deletedStatus);

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
            var newStatus = dto.Status.Trim().ToUpper();
            if (newStatus == deletedStatus || (!Enum.TryParse<JobStatus>(newStatus, out _)))
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
            var deletedStatus = JobStatus.DELETED.ToString();

            // Find existing Job
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id && j.Status != deletedStatus);

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
            job.Status = deletedStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Job deleted successfully" });
        }
    }
}
