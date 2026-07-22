using API.Services.Auth;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects;
using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using LinqKit;
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
        public async Task<ActionResult<PaginateResult<JobDTO>>> GetJobs([FromQuery] FilterJobDTO filter)
        {
            var query = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.EmployerProfile)
                    .ThenInclude(e => e.Account)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Include(j => j.Applications)
                .Where(j => j.Status != JobStatus.DELETED)
                .AsQueryable();

            query = ApplyPermission(query);

            #region Filters

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var tokens = filter.Keyword
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var predicate = PredicateBuilder.New<Job>(false);

                foreach (var t in tokens)
                {
                    var token = t;

                    predicate = predicate.Or(x =>
                        x.Title.Contains(token) ||
                        x.Description.Contains(token));
                }

                query = query.Where(predicate);
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

        private void FullTextSearch()
        {

        }

        [HttpPost("upload-image")]
        [Authorize(Roles = "EMPLOYER")]
        public async Task<IActionResult> UploadJobImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Create directory if not exists
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "jobs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var request = HttpContext.Request;
            var imageUrl = $"{request.Scheme}://{request.Host}/uploads/jobs/{fileName}";

            return Ok(new { location = imageUrl });
        }

        // POST: api/jobs
        [HttpPost]
        [Authorize(Roles = "EMPLOYER")]
        public async Task<ActionResult<JobDTO>> CreateJob([FromBody] CreateJobDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userId = _user.UserId;

            // Resolve Employer Profile
            var employerProfile = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.AccountId == userId);

            if (employerProfile == null)
            {
                return BadRequest(new { message = "Employer profile not found for this user" });
            }

            // Resolve Category (existing ID or new name)
            int resolvedCategoryId = 0;

            if (dto.CategoryId.HasValue && dto.CategoryId.Value > 0)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
                if (!categoryExists)
                {
                    return BadRequest(new { message = "Invalid CategoryId" });
                }
                resolvedCategoryId = dto.CategoryId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(dto.NewCategoryName))
            {
                var trimmedCatName = dto.NewCategoryName.Trim();
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == trimmedCatName.ToLower());
                
                if (existingCategory != null)
                {
                    resolvedCategoryId = existingCategory.Id;
                }
                else
                {
                    var newCategory = new Category { Name = trimmedCatName };
                    _context.Categories.Add(newCategory);
                    await _context.SaveChangesAsync();
                    resolvedCategoryId = newCategory.Id;
                }
            }
            else
            {
                return BadRequest(new { message = "Vui long chon hoac nhap Nganh nghe (Category)" });
            }

            if (dto.Budget <= 0)
            {
                return BadRequest("Budget must be greater than zero.");
            }

            if (dto.Deadline <= DateTime.Now)
            {
                return BadRequest("Deadline must be in the future.");
            }

            // Check wallet balance for Job Posting Fee
            var feeSettings = HttpContext.RequestServices
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<BusinessObjects.Common.ServiceFeeSettings>>().Value;

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null || wallet.Balance < feeSettings.JobPostingFee)
            {
                return BadRequest(new { message = $"So du vi khong du de dang tin. Can it nhat {feeSettings.JobPostingFee:N0} VND." });
            }

            // Map CreateJobDto to Job entity
            var job = new Job
            {
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                Budget = dto.Budget,
                CategoryId = resolvedCategoryId,
                Deadline = dto.Deadline,
                Status = JobStatus.ACTIVE,
                EmployerProfileId = employerProfile.Id,
                CreatedAt = DateTime.Now
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Jobs.Add(job);
                await _context.SaveChangesAsync(); // Saves job and generates its Id

                // Deduct job posting fee
                wallet.Balance -= feeSettings.JobPostingFee;
                wallet.UpdatedAt = DateTime.Now;

                _context.Transactions.Add(new Transaction
                {
                    WalletId = wallet.Id,
                    JobId = job.Id,
                    Type = TransactionType.JOB_POSTING_FEE,
                    Amount = -feeSettings.JobPostingFee,
                    BalanceAfter = wallet.Balance,
                    Description = $"Phi dang tin: {job.Title}"
                });

                await _context.SaveChangesAsync();

                // Combine and resolve existing + new skills
                var allSkillIds = new HashSet<int>();

                // 1. Process existing skill IDs
                if (dto.Skills != null && dto.Skills.Any())
                {
                    var validSkillIds = await _context.Skills
                        .Where(s => dto.Skills.Distinct().ToList().Contains(s.Id))
                        .Select(s => s.Id)
                        .ToListAsync();

                    foreach (var id in validSkillIds)
                    {
                        allSkillIds.Add(id);
                    }
                }

                // 2. Process new skill names (strings)
                if (dto.NewSkills != null && dto.NewSkills.Any())
                {
                    foreach (var skillName in dto.NewSkills.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct())
                    {
                        var existingSkill = await _context.Skills
                            .FirstOrDefaultAsync(s => s.Name.ToLower() == skillName.ToLower());

                        if (existingSkill != null)
                        {
                            allSkillIds.Add(existingSkill.Id);
                        }
                        else
                        {
                            var newSkill = new Skill { Name = skillName };
                            _context.Skills.Add(newSkill);
                            await _context.SaveChangesAsync();
                            allSkillIds.Add(newSkill.Id);
                        }
                    }
                }

                // 3. Associate all resolved skill IDs to the Job
                if (allSkillIds.Any())
                {
                    foreach (var skillId in allSkillIds)
                    {
                        _context.JobSkills.Add(new JobSkill
                        {
                            JobId = job.Id,
                            SkillId = skillId
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
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

            // Resolve Category (existing ID or new name)
            int resolvedCategoryId = 0;

            if (dto.CategoryId.HasValue && dto.CategoryId.Value > 0)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
                if (!categoryExists)
                {
                    return BadRequest(new { message = "Invalid CategoryId" });
                }
                resolvedCategoryId = dto.CategoryId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(dto.NewCategoryName))
            {
                var trimmedCatName = dto.NewCategoryName.Trim();
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == trimmedCatName.ToLower());
                
                if (existingCategory != null)
                {
                    resolvedCategoryId = existingCategory.Id;
                }
                else
                {
                    var newCategory = new Category { Name = trimmedCatName };
                    _context.Categories.Add(newCategory);
                    await _context.SaveChangesAsync();
                    resolvedCategoryId = newCategory.Id;
                }
            }
            else
            {
                return BadRequest(new { message = "Vui long chon hoac nhap Nganh nghe (Category)" });
            }

            // Update Job fields
            job.Title = dto.Title.Trim();
            job.Description = dto.Description.Trim();
            job.Budget = dto.Budget;
            job.CategoryId = resolvedCategoryId;
            job.Deadline = dto.Deadline;

            // Combine and resolve existing + new skills
            var allTargetSkillIds = new HashSet<int>();

            // 1. Process existing skill IDs
            if (dto.Skills != null && dto.Skills.Any())
            {
                var validSkillIds = await _context.Skills
                    .Where(s => dto.Skills.Distinct().ToList().Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync();

                foreach (var skId in validSkillIds)
                {
                    allTargetSkillIds.Add(skId);
                }
            }

            // 2. Process new skill names (strings)
            if (dto.NewSkills != null && dto.NewSkills.Any())
            {
                foreach (var skillName in dto.NewSkills.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct())
                {
                    var existingSkill = await _context.Skills
                        .FirstOrDefaultAsync(s => s.Name.ToLower() == skillName.ToLower());

                    if (existingSkill != null)
                    {
                        allTargetSkillIds.Add(existingSkill.Id);
                    }
                    else
                    {
                        var newSkill = new Skill { Name = skillName };
                        _context.Skills.Add(newSkill);
                        await _context.SaveChangesAsync();
                        allTargetSkillIds.Add(newSkill.Id);
                    }
                }
            }

            // Update Skills association
            var currentSkills = await _context.JobSkills
                .Where(x => x.JobId == job.Id)
                .Select(x => x.SkillId)
                .ToListAsync();

            var toRemove = currentSkills.Except(allTargetSkillIds).ToList();
            var toAdd = allTargetSkillIds.Except(currentSkills).ToList();

            // remove
            var removeEntities = await _context.JobSkills
                .Where(x => x.JobId == job.Id && toRemove.Contains(x.SkillId))
                .ToListAsync();

            _context.JobSkills.RemoveRange(removeEntities);

            // add
            foreach (var skillId in toAdd)
            {
                _context.JobSkills.Add(new JobSkill
                {
                    JobId = job.Id,
                    SkillId = skillId
                });
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

        private IQueryable<Job> ApplyPermission(IQueryable<Job> query)
        {
            switch (_user.Role.ToLower())
            {
                case "admin":
                    return query;

                case "employer":
                    return query.Where(x => x.EmployerProfile.AccountId == _user.UserId);

                case "freelancer":
                default:
                    // Freelancers and anonymous users can only see ACTIVE jobs
                    return query.Where(x => x.Status == JobStatus.ACTIVE);
            }
        }


        [HttpPost("close/{id}")]
        public async Task<IActionResult> Close(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.EmployerProfile)
                    .ThenInclude(e => e.Account)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (job == null)
            {
                return Ok(ApiResult<bool>.Fail("Job not found"));
            }

            if (job.EmployerProfile.AccountId != _user.UserId && _user.Role.ToLower() != "admin")
            {
                return Ok(ApiResult<bool>.Fail("Unauthorized"));
            }

            job.Status = JobStatus.CLOSED;

            await _context.SaveChangesAsync();

            return Ok(ApiResult<bool>.Ok(true, $"Close job({job.Title}) successfully!"));
        }


        [HttpPost("open/{id}")]
        public async Task<IActionResult> Open(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.EmployerProfile)
                    .ThenInclude(e => e.Account)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (job == null)
            {
                return Ok(ApiResult<bool>.Fail("Job not found"));
            }

            if (job.EmployerProfile.AccountId != _user.UserId && _user.Role.ToLower() != "admin")
            {
                return Ok(ApiResult<bool>.Fail("Unauthorized"));
            }

            job.Status = JobStatus.ACTIVE;

            await _context.SaveChangesAsync();

            return Ok(ApiResult<bool>.Ok(true, $"Open job({job.Title}) successfully!"));
        }

        [HttpPost("admin/toggle-status/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AdminToggleStatus(int id)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(x => x.Id == id);
            if (job == null)
            {
                return Ok(ApiResult<bool>.Fail("Job not found"));
            }

            if (job.Status == JobStatus.DELETED)
            {
                job.Status = JobStatus.ACTIVE;
            }
            else
            {
                job.Status = JobStatus.DELETED;
            }

            await _context.SaveChangesAsync();
            return Ok(ApiResult<bool>.Ok(true, $"Toggled job status to {job.Status} successfully!"));
        }
    }
}
