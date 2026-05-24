using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.DTOs;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        
        public FreelancerController(AppDbContext context, IMapper mapper, IUserService user)
        {
            _context = context;
            _mapper = mapper;
            _user = user;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = _user.UserId;
            
            var profile = await _context.FreelancerProfiles.Include(f => f.Account).FirstOrDefaultAsync(x => x.AccountId == userId);
            if (profile == null)
            {
                profile = new FreelancerProfile()
                {
                    AccountId = userId,
                    Title = "",
                    Bio = ""
                };
                _context.FreelancerProfiles.Add(profile);
                await _context.SaveChangesAsync();
                
                profile = await _context.FreelancerProfiles
                    .Include(p => p.Account)
                    .FirstOrDefaultAsync(p => p.AccountId == userId);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            var dto = _mapper.Map<FreelancerDto>(profile);
            dto.Email = user.Email;

            var skillIds = await _context.FreelancerSkills
                .Where(fs => fs.FreelancerProfileId == profile.Id)
                .Select(f => f.SkillId)
                .ToListAsync();
            var skills = await _context.Skills
                .Where(s => skillIds.Contains(s.Id))
                .ToListAsync();
            dto.Skills = _mapper.Map<List<SkillDTO>>(skills);
            return Ok(dto);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateFreelancerProfileDto dto)
        {
            var userId = _user.UserId;
            var profile = await _context.FreelancerProfiles.FirstOrDefaultAsync(p => p.AccountId == userId);
            
            if(profile == null)
                return NotFound("Profile not found");
            
            bool isEmailExist = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            bool isPhoneExist = await _context.FreelancerProfiles.AnyAsync(f => f.Phone == dto.PhoneNumber);
            if(isEmailExist)
                return BadRequest("Email already exists");
            if(isPhoneExist)
                return BadRequest("Phone number already exists");
            
            profile.Title = dto.Title;
            profile.Bio = dto.Bio;
            profile.Address = dto.Address;
            profile.Account.Email = dto.Email;
            profile.Phone = dto.PhoneNumber;
            profile.CVUrl = dto.CVUrl;
            profile.PortfolioUrl = dto.PortfolioUrl;
            profile.PortfolioDescription = dto.PortfolioDescription;
            
            var existingSkills = _context.FreelancerSkills.Where(f => f.FreelancerProfileId == profile.Id).ToList();
            _context.FreelancerSkills.RemoveRange(existingSkills);

            if (dto.Skills != null && dto.Skills.Any())
            {
                foreach (var skill in dto.Skills)
                {
                    _context.FreelancerSkills.Add(new FreelancerSkill
                    {
                        FreelancerProfileId = profile.Id,
                        SkillId = skill.Id
                    });
                }
            }
            
            await _context.SaveChangesAsync();
            return Ok(new { message = "Update profile successfully"});
        }
    }
}
