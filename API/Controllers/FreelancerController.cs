using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.DTOs;
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
                var newProfile = new FreelancerProfile { AccountId = userId, Title = "", Bio = "" };
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
                var phoneExists = await _context.FreelancerProfiles.AnyAsync(f => f.Phone == dto.Phone && f.AccountId != userId);
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
            if (file == null || file.Length == 0) return BadRequest("Invalid file");
            var extension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
            if (!allowedExtensions.Contains(extension)) return BadRequest("Invalid extension");
            if (file.Length > 2 * 1024 * 1024) return BadRequest("File is too large (max 2MB)");
            var userId = _user.UserId;
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(f => f.AccountId == userId);
            if (profile == null) return NotFound("Profile not found");
            var uploadDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "avatar");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
            
            if (!string.IsNullOrEmpty(profile.ProfilePhoto))
            {
                var oldFilePath = Path.Combine(uploadDir, Path.GetFileName(profile.ProfilePhoto));
                if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
            }
            var uniqueFileName = $"Avatar_{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var request = HttpContext.Request;
            var fileUrl = $"{request.Scheme}://{request.Host}/uploads/avatar/{uniqueFileName}";
            profile.ProfilePhoto = fileUrl;
            await _context.SaveChangesAsync();
            return Ok(new { avatarUrl = fileUrl, message = "Upload avatar successfully" });
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
            if (file == null || file.Length == 0) return BadRequest("Invalid file");
            if (file.Length > 5 * 1024 * 1024) return BadRequest("File too large (max 5MB)");
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".pdf") return BadRequest("Only PDF files are allowed");
            var userId = _user.UserId;
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == userId);
            if (profile == null) return NotFound("Profile not found");
            var uploadDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "cv");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
            // Xóa file CV cũ
            if (!string.IsNullOrEmpty(profile.CVUrl))
            {
                var oldFilePath = Path.Combine(uploadDir, Path.GetFileName(profile.CVUrl));
                if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
            }
            var uniqueName = $"CV_{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDir, uniqueName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var request = HttpContext.Request;
            var fileUrl = $"{request.Scheme}://{request.Host}/uploads/cv/{uniqueName}";
            profile.CVUrl = fileUrl;
            await _context.SaveChangesAsync();
            return Ok(new { cvUrl = fileUrl, message = "Upload CV successfully" });
        }
    }
}
