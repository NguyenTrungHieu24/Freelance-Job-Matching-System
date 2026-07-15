using API.Helper;
using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/freelancer")]
    [ApiController]
    [Authorize(Roles = "FREELANCER")]
    public class FreelancerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _user;
        private readonly IWebHostEnvironment _env;

        public FreelancerController(AppDbContext context, IMapper mapper, IUserService user, IWebHostEnvironment env)
        {
            _context = context;
            _mapper = mapper;
            _user = user;
            _env = env;
        }

        [HttpGet("personal-info")]
        public async Task<IActionResult> GetPersonalInfo()
        {
            var userId = _user.UserId;
            var user = await _context.Users
                .Include(u => u.FreelancerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("User not found");

            if (user.FreelancerProfile == null)
            {
                var newProfile = new FreelancerProfile
                    { AccountId = userId, Title = "New Freelancer", Bio = "New Bio" };
                _context.FreelancerProfiles.Add(newProfile);
                await _context.SaveChangesAsync();
            }

            var dto = new FreelancerPersonalInfoDto
            {
                AccountId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.FreelancerProfile?.Phone,
                Address = user.FreelancerProfile?.Address,
                ProfilePhoto = user.FreelancerProfile?.ProfilePhoto
            };
            return Ok(dto);
        }

        [HttpPut("personal-info")]
        public async Task<IActionResult> UpdatePersonalInfo([FromBody] UpdateFreelancerPersonalInfoDto dto)
        {
            var userId = _user.UserId;
            var user = await _context.Users
                .Include(u => u.FreelancerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("User not found");

            var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId);
            if (emailExists) return BadRequest("Email already exists");

            if (!string.IsNullOrEmpty(dto.Phone))
            {
                var phoneExists =
                    await _context.FreelancerProfiles.AnyAsync(f => f.Phone == dto.Phone && f.AccountId != userId);
                if (phoneExists) return BadRequest("Phone number already exists");
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;

            if (user.FreelancerProfile != null)
            {
                user.FreelancerProfile.Phone = dto.Phone;
                user.FreelancerProfile.Address = dto.Address;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Update personal information successfully" });
        }

        [HttpPost("personal-info/avatar-upload")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == _user.UserId);
            if (profile == null) return NotFound();
            if (!FileValidateHelper.IsAvatarValid(file))
                return BadRequest();
            if (file.Length > 0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatar", file.FileName);
                using (var stream = System.IO.File.Create(path))
                {
                    await file.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                profile.ProfilePhoto = $"{request.Scheme}://{request.Host}/uploads/avatar/" + file.FileName;
            }
            else
            {
                profile.ProfilePhoto = "";
            }

            await _context.SaveChangesAsync();
            return Ok(new { avatarUrl = profile.ProfilePhoto, message = "Upload avatar successfully" });
        }

        [HttpGet("cv-portfolio")]
        public async Task<IActionResult> GetCvPortfolio()
        {
            var userId = _user.UserId;
            var profile = await _context.FreelancerProfiles
                .FirstOrDefaultAsync(p => p.AccountId == userId);
            if (profile == null)
            {
                profile = new FreelancerProfile { AccountId = userId, Title = "", Bio = "" };
                _context.FreelancerProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            var dto = new FreelancerCvDto
            {
                ProfileId = profile.Id,
                Title = profile.Title,
                Bio = profile.Bio,
                CVUrl = profile.CVUrl,
                PortfolioUrl = profile.PortfolioUrl,
                PortfolioDescription = profile.PortfolioDescription
            };

            var skillIds = await _context.FreelancerSkills
                .Where(fs => fs.FreelancerProfileId == profile.Id)
                .Select(fs => fs.SkillId)
                .ToListAsync();
            var skills = await _context.Skills
                .Where(s => skillIds.Contains(s.Id))
                .ToListAsync();
            dto.Skills = _mapper.Map<List<SkillDTO>>(skills);
            return Ok(dto);
        }

        [HttpPut("cv-portfolio")]
        public async Task<IActionResult> UpdateCvPortfolio([FromBody] UpdateFreelancerCvDto dto)
        {
            var userId = _user.UserId;
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == userId);
            if (profile == null) return NotFound("Profile not found");
            profile.Title = dto.Title;
            profile.Bio = dto.Bio;
            profile.PortfolioUrl = dto.PortfolioUrl;
            profile.PortfolioDescription = dto.PortfolioDescription;

            var existingSkills = _context.FreelancerSkills.Where(fs => fs.FreelancerProfileId == profile.Id);
            _context.FreelancerSkills.RemoveRange(existingSkills);
            if (dto.Skills != null && dto.Skills.Any())
            {
                foreach (var skillId in dto.Skills)
                {
                    _context.FreelancerSkills.Add(new FreelancerSkill
                    {
                        FreelancerProfileId = profile.Id,
                        SkillId = skillId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Update CV & Portfolio successfully" });
        }

        [HttpPost("cv-portfolio/cv-upload")]
        public async Task<IActionResult> UploadCvFile(IFormFile file)
        {
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == _user.UserId);
            if (profile == null) return NotFound();
            if (!FileValidateHelper.IsCvFileValid(file))
                return BadRequest();
            if (file.Length > 0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cv", file.FileName);
                using (var stream = System.IO.File.Create(path))
                {
                    await file.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                profile.CVUrl = $"{request.Scheme}://{request.Host}/uploads/cv/" + file.FileName;
            }
            else
            {
                profile.CVUrl = "";
            }

            await _context.SaveChangesAsync();
            return Ok(new { cvUrl = profile.CVUrl, message = "Upload CV successfully" });
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobs([FromQuery] FreelancerFilterJobDTO filter)
        {
            var userId = _user.UserId;
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == userId);

            var query = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.JobSkills).ThenInclude(s => s.Skill)
                .Include(j => j.Applications)
                .Include(j => j.EmployerProfile).ThenInclude(e => e.Account)
                .Where(j => j.Status != JobStatus.DELETED && j.Status == JobStatus.ACTIVE)
                .AsQueryable();

            if (profile != null)
            {
                query = query.Where(j => !j.Applications.Any(a => a.FreelancerProfileId == profile.Id));
            }
            
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                query = query.Where(x => x.Title.Contains(filter.Keyword) || x.Description.Contains(filter.Keyword));
            }
            if (filter.CategoryId.HasValue)
                query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
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
            if (filter.SkillIds.Count > 0)
                query = query.Where(x => x.JobSkills.Any(js => filter.SkillIds.Contains(js.SkillId)));
            if (filter.MinBudget.HasValue)
                query = query.Where(x => x.Budget >= filter.MinBudget.Value);
            if (filter.MaxBudget.HasValue)
                query = query.Where(x => x.Budget <= filter.MaxBudget.Value);
            if (filter.CreatedFrom.HasValue)
                query = query.Where(x => x.CreatedAt >= filter.CreatedFrom.Value);
            if (filter.CreatedTo.HasValue)
                query = query.Where(x => x.CreatedAt <= filter.CreatedTo.Value);
            if (filter.DeadlineFrom.HasValue)
                query = query.Where(x => x.Deadline >= filter.DeadlineFrom.Value);
            if (filter.DeadlineTo.HasValue)
                query = query.Where(x => x.Deadline <= filter.DeadlineTo.Value);
            
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
            
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(e => new FreelancerJobDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Budget = e.Budget,
                    Deadline = e.Deadline,
                    CreatedAt = e.CreatedAt,
                    EmployerProfileId = e.EmployerProfileId,
                    CategoryName = e.Category.Name,
                    Skills = e.JobSkills.Select(x => x.Skill.Name).ToList(),
                    EmployerName = e.EmployerProfile.Account.FullName,
                    EmployerLogo = e.EmployerProfile.Logo,
                    CompanyName = e.EmployerProfile.CompanyName,
                    JobStatus = e.Status,
                    ApplicationsCount = e.Applications.Where(a => a.JobId == e.Id).Count(),
                })
                .ToListAsync();
            
            var result = new PaginateResult<FreelancerJobDTO>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };
            return Ok(result);
        }

        [HttpGet("jobs/{id}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            var userId = _user.UserId;
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == userId);
            
            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.JobSkills).ThenInclude(s => s.Skill)
                .Include(j => j.Applications)
                .Include(j => j.EmployerProfile).ThenInclude(e => e.Account)
                .FirstOrDefaultAsync(j => j.Id == id && j.Status == JobStatus.ACTIVE);

            if (job == null) return NotFound("Job not found or inactive.");

            var employerPostedJobsCount = await _context.Jobs
                .CountAsync(j => j.EmployerProfileId == job.EmployerProfileId && j.Status == JobStatus.ACTIVE);

            var application = profile != null ? job.Applications.FirstOrDefault(a => a.FreelancerProfileId == profile.Id) : null;

            var dto = new FreelancerJobDTO
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Budget = job.Budget,
                Deadline = job.Deadline,
                CreatedAt = job.CreatedAt,
                EmployerProfileId = job.EmployerProfileId,
                CategoryName = job.Category?.Name ?? "",
                Skills = job.JobSkills.Select(x => x.Skill.Name).ToList(),
                EmployerName = job.EmployerProfile?.Account?.FullName ?? "",
                CompanyName = job.EmployerProfile?.CompanyName ?? "",
                EmployerLogo = job.EmployerProfile?.Logo ?? "",
                PostedJobCount = employerPostedJobsCount,
                IsApplied = application != null,
                JobStatus = job.Status,
                ApplyStatus = application?.Status,
                ApplicationsCount = job.Applications.Count
            };

            return Ok(dto);
        }

        [HttpPost("jobs/apply")]
        public async Task<IActionResult> ApplyJob([FromForm] CreateApplicationDto dto, IFormFile? cvFile)
        {
            var userId = _user.UserId;
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == userId);
            if (profile == null) return BadRequest("Freelancer profile not found.");

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == dto.JobId);
            if (job == null || job.Status != JobStatus.ACTIVE)
                return BadRequest("Job is not available.");

            var existingApplication = await _context.Applications
                .AnyAsync(a => a.JobId == dto.JobId && a.FreelancerProfileId == profile.Id);
            if (existingApplication)
                return BadRequest("You have already applied for this job.");

            string? cvUrl = null;
            if (cvFile != null && cvFile.Length > 0)
            {
                if (!FileValidateHelper.IsCvFileValid(cvFile))
                    return BadRequest("Invalid CV file. Only PDF, DOC, DOCX files are allowed.");

                if (cvFile.Length > 5 * 1024 * 1024)
                    return BadRequest("CV file size is too large (max 5MB).");

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(cvFile.FileName).ToLower()}";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cv", fileName);

                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);

                using (var stream = System.IO.File.Create(path))
                {
                    await cvFile.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                cvUrl = $"{request.Scheme}://{request.Host}/uploads/cv/{fileName}";
            }

            var application = new Application
            {
                JobId = dto.JobId,
                FreelancerProfileId = profile.Id,
                CoverLetter = dto.CoverLetter,
                CvUrl = cvUrl,
                Status = ApplicationStatus.PENDING,
                AppliedAt = DateTime.Now
            };

            _context.Applications.Add(application);
            
            var user = await _context.Users.FindAsync(userId);
            var employer = await _context.EmployerProfiles.FindAsync(job.EmployerProfileId);
            if (user != null && employer != null)
            {
                var notification = new Notification
                {
                    AccountId = employer.AccountId,
                    Content = $"Freelancer {user.FullName} has applied for your job: '{job.Title}'.",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            return Ok(true);
        }

        [HttpGet("applications")]
        public async Task<IActionResult> GetApplicationHistory([FromQuery] ApplicationStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = _user.UserId;
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == userId);
            if (profile == null) return BadRequest("Freelancer not found");
            
            var query = _context.Applications
                .Include(a => a.Job).ThenInclude(j => j.EmployerProfile)
                .ThenInclude(e => e.Account)
                .Where(a => a.FreelancerProfileId == profile.Id);
            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            query = query.OrderByDescending(a => a.AppliedAt);
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dto = _mapper.Map<List<ApplicationHistoryDto>>(items);
            var rs = new PaginateResult<ApplicationHistoryDto>
            {
                Items = dto,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize
            };
            return Ok(rs);
        }

        [HttpPut("application/{id}/cancel")]
        public async Task<IActionResult> CancelApply(int id)
        {
            var user = _user.UserId;
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == user);
            if (profile == null) return BadRequest("Profile not found");

            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.FreelancerProfileId == profile.Id && a.Status == ApplicationStatus.PENDING);

            if (application == null) return NotFound("Application not found or cannot be cancelled.");
            
            application.Status = ApplicationStatus.CANCELLED;
            _context.Applications.Update(application);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Application cancelled successfully" });
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = _user.UserId;
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == userId);
            if (profile == null) return BadRequest("Freelancer not found");

            var applicationsQuery = _context.Applications.Where(a => a.FreelancerProfileId == profile.Id);
            
            var totalApplications = await applicationsQuery.CountAsync();
            var pendingApplications = await applicationsQuery.CountAsync(a => a.Status == ApplicationStatus.PENDING);
            var acceptedApplications = await applicationsQuery.CountAsync(a => a.Status == ApplicationStatus.ACCEPTED);
            var rejectedApplications = await applicationsQuery.CountAsync(a => a.Status == ApplicationStatus.REJECTED);
            var cancelledApplications = await applicationsQuery.CountAsync(a => a.Status == ApplicationStatus.CANCELLED);

            var totalEarnings = await _context.Payments
                .Include(p => p.Application)
                .Where(p => p.Application.FreelancerProfileId == profile.Id && p.Status == PaymentStatus.PAID)
                .SumAsync(p => p.Amount);

            var recentApps = await applicationsQuery
                .Include(a => a.Job).ThenInclude(j => j.EmployerProfile).ThenInclude(e => e.Account)
                .OrderByDescending(a => a.AppliedAt)
                .Take(5)
                .ToListAsync();

            var freelancerSkills = await _context.FreelancerSkills
                .Where(fs => fs.FreelancerProfileId == profile.Id)
                .Select(fs => fs.SkillId)
                .ToListAsync();

            var recommendedJobsQuery = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.JobSkills).ThenInclude(s => s.Skill)
                .Include(j => j.EmployerProfile).ThenInclude(e => e.Account)
                .Include(j => j.Applications)
                .Where(j => j.Status == JobStatus.ACTIVE);

            if (freelancerSkills.Any())
            {
                recommendedJobsQuery = recommendedJobsQuery
                    .Where(j => j.JobSkills.Any(js => freelancerSkills.Contains(js.SkillId)));
            }

            var recommendedJobs = await recommendedJobsQuery
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .ToListAsync();

            var dto = new FreelancerDashboardDto
            {
                TotalApplications = totalApplications,
                PendingApplications = pendingApplications,
                AcceptedApplications = acceptedApplications,
                RejectedApplications = rejectedApplications,
                CancelledApplications = cancelledApplications,
                TotalEarnings = totalEarnings,
                RecentApplications = _mapper.Map<List<ApplicationHistoryDto>>(recentApps),
                RecommendedJobs = recommendedJobs.Select(e => new FreelancerJobDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Budget = e.Budget,
                    Deadline = e.Deadline,
                    CreatedAt = e.CreatedAt,
                    EmployerProfileId = e.EmployerProfileId,
                    CategoryName = e.Category?.Name ?? "",
                    Skills = e.JobSkills.Select(x => x.Skill.Name).ToList(),
                    EmployerName = e.EmployerProfile?.Account?.FullName ?? "",
                    EmployerLogo = e.EmployerProfile?.Logo ?? "",
                    CompanyName = e.EmployerProfile?.CompanyName ?? "",
                    ApplicationsCount = e.Applications.Count
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpGet("my-jobs")]
        public async Task<IActionResult> GetMyJobs()
        {
            var userId = _user.UserId;
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == userId);
            if (profile == null) return NotFound();

            var application = await _context.Applications
                .Include(a => a.Job).ThenInclude(j => j.EmployerProfile)
                .ThenInclude(e => e.Account)
                .Where(a => a.FreelancerProfileId == profile.Id)
                .ToListAsync();

            var myJobs = new List<MyJobDto>();
            foreach (var a in application)
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.ApplicationId == a.Id);
                int employerId = a.Job?.EmployerProfile?.AccountId ?? 0;
                var isReviewed = false;
                if (employerId > 0)
                {
                    isReviewed = await _context.Reviews.AnyAsync(r =>
                        r.ReviewerId == employerId && r.RevieweeId == userId);
                }

                var dto = new MyJobDto
                {
                    ApplicationId = a.Id,
                    JobId = a.JobId,
                    JobTitle = a.Job?.Title ?? "",
                    EmployerName = a.Job?.EmployerProfile?.Account?.FullName ?? "",
                    EmployerId = a.Job?.EmployerProfile?.AccountId ?? 0,
                    CompanyName = a.Job?.EmployerProfile?.CompanyName ?? "",
                    Budget = a.Job?.Budget ?? 0,
                    AppliedAt = a.AppliedAt,
                    Status = a.Status,
                    JobStatus = a.Job?.Status ?? JobStatus.ACTIVE,
                    PaymentStatus = payment?.Status,
                    IsReviewed = isReviewed,
                };

                if (a.Status == ApplicationStatus.CANCELLED || a.Status == ApplicationStatus.REJECTED)
                    dto.ProgressStage = -1;
                else if(a.Status == ApplicationStatus.ACCEPTED && a.Job.Status == JobStatus.ACTIVE)
                    dto.ProgressStage = 1;
                else if (a.Status == ApplicationStatus.ACCEPTED && a.Job.Status == JobStatus.CLOSED)
                {
                    if(payment == null || payment.Status == PaymentStatus.PENDING)
                        dto.ProgressStage = 2;
                    else if (payment.Status == PaymentStatus.PAID)
                        dto.ProgressStage = 3;
                }
                myJobs.Add(dto);
            }

            return Ok(myJobs.OrderByDescending(j => j.AppliedAt));
        }

        [HttpPost("report")]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportDto dto)
        {
            var reporterId = _user.UserId;
            var report = new Report
            {
                ReporterId = reporterId,
                ReportedUserId = dto.ReportUserId,
                Reason = dto.Reason,
                Description = dto.Description,
                Status = ReportStatus.PENDING,
                CreatedAt = DateTime.UtcNow
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return Ok(new { messsage = "Report submitted successfully" });
        }

        [HttpGet("reviews")]
        public async Task<IActionResult> GetReviews()
        {
            var userId = _user.UserId;
            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.RevieweeId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var dto = _mapper.Map<List<ReviewDto>>(reviews);
            return Ok(dto);
        }
    }
}