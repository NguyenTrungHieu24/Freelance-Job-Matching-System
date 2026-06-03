using API.Helper;
using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.DTOs;
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
                Description =  "New Description",
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
    
}