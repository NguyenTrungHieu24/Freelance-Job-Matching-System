using API.Helper;
using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Enums;
namespace API.Controllers;

[Route("api/employer")]
[ApiController]
[Authorize(Roles = "EMPLOYER")]
public class EmployerController : BaseController
{
    public EmployerController(AppDbContext context, IMapper mapper, IUserService user) : base(context, mapper, user)
    {

    }

    [HttpGet("personal-info")]
    public async Task<IActionResult> GetPersonalInfo()
    {
        var userId = _user.UserId;
        var user = await _context.Users.Include(c => c.EmployerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();
        if (user.EmployerProfile == null)
        {
            var newProfile = new EmployerProfile
            {
                AccountId = userId,
                CompanyName = "New Company",
                Description = "New Description",
            };
            _context.EmployerProfiles.Add(newProfile);
            await _context.SaveChangesAsync();
        }

        var dto = new EmployerDto
        {
            AccountId = user.Id,
            FullName = user.FullName,
            CompanyName = user.EmployerProfile?.CompanyName ?? "",
            Description = user.EmployerProfile?.Description ?? "",
            Email = user.Email,
            Phone = user.EmployerProfile?.Phone,
            Address = user.EmployerProfile?.Address,
            Logo = user.EmployerProfile?.Logo ?? ""
        };
        return Ok(dto);
    }

    [HttpPut("personal-info")]
    public async Task<IActionResult> UpdateEmployerProfile([FromBody] UpdateEmployerProfileDto dto)
    {
        var userId = _user.UserId;
        var user = await _context.Users.Include(c => c.EmployerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound(new { message = "User not found" });

        var isEmailExist = await _context.Users.AnyAsync(e => e.Email == dto.Email && e.Id != userId);
        if (isEmailExist) return BadRequest(new { message = "Email already in use" });

        if (!string.IsNullOrEmpty(dto.Phone))
        {
            if (dto.Phone.Length > 12)
                return BadRequest(new { message = "Phone number is too long (max 12 characters)" });
            var isPhoneExist = await _context.EmployerProfiles
                .AnyAsync(e => e.Phone == dto.Phone && e.AccountId != userId);
            if (isPhoneExist)
                return BadRequest(new { message = "Phone number already in use" });
        }

        user.FullName = dto.FullName ?? user.FullName;
        user.Email = dto.Email;

        if (user.EmployerProfile != null)
        {
            user.EmployerProfile.CompanyName = dto.CompanyName;
            user.EmployerProfile.Description = dto.Description;
            user.EmployerProfile.Phone = dto.Phone;
            user.EmployerProfile.Address = dto.Address;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Updated personal info successfully" });
    }

    [HttpPost("personal-info/logo-upload")]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.AccountId == _user.UserId);
        if (profile == null) return NotFound();

        if (!FileValidateHelper.IsAvatarValid(file)) return BadRequest();
        if (file.Length > 0)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logo", file.FileName);
            using (var stream = System.IO.File.Create(path))
            {
                await file.CopyToAsync(stream);
            }
            var request = HttpContext.Request;
            profile.Logo = $"{request.Scheme}://{request.Host}/uploads/logo/" + file.FileName;
        }
        else
        {
            profile.Logo = "";
        }
        await _context.SaveChangesAsync();
        return Ok(new { message = "Logo upload successfully" });
    }




    // GET: api/employer/applications
    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications()
    {
        var userId = _user.UserId;

        // Lay thong tin Profile Employer cua User hien tai
        var employer = await _context.EmployerProfiles
            .FirstOrDefaultAsync(e => e.AccountId == userId);

        if (employer == null)
            return BadRequest(new { message = "Employer profile not found." });

        // Truy van tat ca application gui den Job cua Employer nay
        var applications = await _context.Applications
            .Include(a => a.Job)
            .Include(a => a.FreelancerProfile)
                .ThenInclude(f => f.Account)
            .Where(a => a.Job.EmployerProfileId == employer.Id)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new EmployerApplicationDto
            {
                Id = a.Id,
                JobId = a.JobId,
                JobTitle = a.Job.Title,
                FreelancerProfileId = a.FreelancerProfileId,
                FreelancerName = a.FreelancerProfile.Account.FullName,
                CoverLetter = a.CoverLetter,
                Status = a.Status.ToString(),
                AppliedAt = a.AppliedAt
            })
            .ToListAsync();

        return Ok(applications);
    }

    // PUT: api/employer/applications/{id}/status
    [HttpPut("applications/{id}/status")]
    public async Task<IActionResult> UpdateApplicationStatus(int id, [FromQuery] ApplicationStatus status)
    {
        var userId = _user.UserId;

        // Kiem tra trang thai Duyet (ACCEPTED hoac REJECTED)
        if (status != ApplicationStatus.ACCEPTED && status != ApplicationStatus.REJECTED)
        {
            return BadRequest(new { message = "Trạng thái duyệt không hợp lệ." });
        }

        // Tim Application va Employer
        var application = await _context.Applications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return NotFound(new { message = "Đơn ứng tuyển không tồn tại." });

        // Xac thuc Job co phai cua Employer dang login khong
        var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.AccountId == userId);
        if (employer == null || application.Job.EmployerProfileId != employer.Id)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Bạn không có quyền duyệt đơn này." });
        }

        // Update trang thai
        if (status == ApplicationStatus.ACCEPTED)
        {
            application.Status = ApplicationStatus.IN_PROGRESS;
            application.Job.Status = JobStatus.IN_PROGRESS;

            // Reject other pending applications for this job
            var otherPending = await _context.Applications
                .Where(a => a.JobId == application.JobId
                         && a.Id != application.Id
                         && a.Status == ApplicationStatus.PENDING)
                .ToListAsync();

            foreach (var other in otherPending)
            {
                other.Status = ApplicationStatus.REJECTED;
            }
        }
        else
        {
            application.Status = status;
        }

        _context.Applications.Update(application);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Đã cập nhật trạng thái đơn ứng tuyển thành: {application.Status}" });
    }


    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = _user.UserId;

        var profile = await _context.EmployerProfiles
            .FirstOrDefaultAsync(e => e.AccountId == userId);

        if (profile == null)
            return BadRequest("Employer not found");

        var jobsQuery = _context.Jobs
            .Where(j => j.EmployerProfileId == profile.Id);

        var totalJobs = await jobsQuery.CountAsync();

        var activeJobs = await jobsQuery.CountAsync(j =>
            j.Status == JobStatus.ACTIVE);

        var jobIds = await jobsQuery
            .Select(j => j.Id)
            .ToListAsync();

        var applicationsQuery = _context.Applications
            .Include(a => a.Job)
            .Include(a => a.FreelancerProfile)
                .ThenInclude(f => f.Account)
            .Where(a => a.Job.EmployerProfileId == profile.Id);

        var totalApplications = await applicationsQuery.CountAsync();

        var pendingApplications = await applicationsQuery.CountAsync(a =>
            a.Status == ApplicationStatus.PENDING);

        var recentJobs = await jobsQuery
            .Include(j => j.JobSkills)
                .ThenInclude(js => js.Skill)
            .Include(j => j.Applications)
            .OrderByDescending(j => j.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentApplications = await applicationsQuery
            .OrderByDescending(a => a.AppliedAt)
            .Take(5)
            .ToListAsync();

        var dto = new EmployerDashboardDto
        {
            TotalJobs = totalJobs,

            ActiveJobs = activeJobs,

            TotalApplications = totalApplications,

            PendingApplications = pendingApplications,

            RecentJobs = recentJobs.Select(j => new EmployerRecentJobDto
            {
                Id = j.Id,
                Title = j.Title,
                Deadline = j.Deadline,
                ApplicationCount = j.Applications.Count,
                IsActive = j.Status == JobStatus.ACTIVE,
                Skills = j.JobSkills
                    .Select(x => x.Skill.Name)
                    .ToList()
            }).ToList(),

            RecentApplications = recentApplications.Select(a => new EmployerRecentApplicationDto
            {
                Id = a.Id,
                JobId = a.JobId,
                JobTitle = a.Job.Title,
                CandidateName = a.FreelancerProfile.Account.FullName,
                CandidateAvatar = a.FreelancerProfile?.ProfilePhoto ?? "",
                AppliedAt = a.AppliedAt,
                Status = a.Status
            }).ToList()
        };

        return Ok(dto);
    }

    /// <summary>
    /// Employer xác nhận hoàn thành Job → Thanh toán tự động
    /// </summary>
    [HttpPost("applications/{id}/complete")]
    public async Task<IActionResult> CompleteApplication(int id)
    {
        var userId = _user.UserId;
        var feeSettings = HttpContext.RequestServices
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<BusinessObjects.Common.ServiceFeeSettings>>().Value;

        var application = await _context.Applications
            .Include(a => a.Job)
            .Include(a => a.FreelancerProfile)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return NotFound("Application not found");

        // Verify ownership
        var employer = await _context.EmployerProfiles
            .FirstOrDefaultAsync(e => e.AccountId == userId);
        if (employer == null || application.Job.EmployerProfileId != employer.Id)
            return Forbid();

        // Verify status
        if (application.Status != ApplicationStatus.IN_PROGRESS)
            return BadRequest("Only IN_PROGRESS applications can be completed");

        var budget = application.Job.Budget;
        var commission = budget * feeSettings.CommissionPercent / 100;
        var freelancerReceives = budget - commission;

        // Check employer wallet
        var employerWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);
        if (employerWallet == null || employerWallet.Balance < budget)
            return BadRequest($"Số dư ví không đủ. Cần {budget:N0} VNĐ");

        // Check freelancer wallet
        var freelancerWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == application.FreelancerProfile.AccountId);
        if (freelancerWallet == null)
            return BadRequest("Freelancer wallet not found");

        // --- THANH TOÁN ---
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Trừ ví Employer
            employerWallet.Balance -= budget;
            employerWallet.UpdatedAt = DateTime.Now;
            _context.Transactions.Add(new Transaction
            {
                WalletId = employerWallet.Id,
                JobId = application.JobId,
                Type = TransactionType.JOB_PAYMENT,
                Amount = -budget,
                BalanceAfter = employerWallet.Balance,
                Description = $"Thanh toán Job: {application.Job.Title}"
            });

            // 2. Cộng ví Freelancer (sau khi trừ hoa hồng)
            freelancerWallet.Balance += freelancerReceives;
            freelancerWallet.UpdatedAt = DateTime.Now;
            _context.Transactions.Add(new Transaction
            {
                WalletId = freelancerWallet.Id,
                JobId = application.JobId,
                Type = TransactionType.JOB_EARNING,
                Amount = freelancerReceives,
                BalanceAfter = freelancerWallet.Balance,
                Description = $"Thu nhập Job: {application.Job.Title} (đã trừ {feeSettings.CommissionPercent}% hoa hồng)"
            });

            // 3. Ghi nhận hoa hồng hệ thống
            _context.Transactions.Add(new Transaction
            {
                WalletId = freelancerWallet.Id,
                JobId = application.JobId,
                Type = TransactionType.COMMISSION_FEE,
                Amount = -commission,
                BalanceAfter = freelancerWallet.Balance,
                Description = $"Phí hoa hồng {feeSettings.CommissionPercent}%: {application.Job.Title}"
            });

            // 4. Cập nhật trạng thái
            application.Status = ApplicationStatus.COMPLETED;
            application.Job.Status = JobStatus.COMPLETED;

            // 5. Thông báo cho Freelancer
            _context.Notifications.Add(new Notification
            {
                AccountId = application.FreelancerProfile.AccountId,
                Content = $"Job \"{application.Job.Title}\" đã hoàn thành! Bạn nhận được {freelancerReceives:N0} VNĐ (sau phí {feeSettings.CommissionPercent}%)."
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Job completed & payment processed!",
                budget,
                commission,
                freelancerReceives
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}