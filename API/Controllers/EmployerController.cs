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
}