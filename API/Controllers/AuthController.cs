using API.Services;
using API.Services.Auth;
using BusinessObjects;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly IUserService _user;

        public AuthController(AppDbContext context, JwtService jwt, IUserService user)
        {
            _context = context;
            _jwt = jwt;
            _user = user;
        }

        // =========================
        // REGISTER
        // =========================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var exists = await _context.Users
                .AnyAsync(x => x.Email == dto.Email.Trim().ToLower());

            if (exists)
                return BadRequest("Email already exists");

            var account = new User
            {
                FullName = dto.Name,
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = dto.Role ?? (int)RoleEnum.EMPLOYER
            };

            _context.Users.Add(account);
            await _context.SaveChangesAsync();


            switch (dto.Role)
            {
                case 3:
                    var f = new FreelancerProfile
                    {
                        AccountId = account.Id,
                    };
                    _context.FreelancerProfiles.Add(f);
                    await _context.SaveChangesAsync();
                    break;
                case 2:
                    var e = new EmployerProfile
                    {
                        AccountId = account.Id
                    };
                    _context.EmployerProfiles.Add(e);
                    await _context.SaveChangesAsync();
                    break;
            }

            await _context.Entry(account)
                .Reference(x => x.Role)
                .LoadAsync();

            var token = _jwt.GenerateToken(account);

            return Ok(new
            {
                Token = token,
                Role = account.Role,
                User = new
                {
                    Name = account.FullName,
                    Email = account.Email
                }
            });
        }

        // =========================
        // LOGIN
        // =========================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email.Trim().ToLower());

            if (user == null)
                return Unauthorized("Invalid email");

            var check = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!check)
                return Unauthorized("Invalid password");

            await _context.Entry(user)
                .Reference(x => x.Role)
                .LoadAsync();

            var token = _jwt.GenerateToken(user);

            return Ok(new
            {
                Token = token,
                Role = user.Role.Name,
                User = new
                {
                    Name = user.FullName,
                    Email = user.Email,
                    Role = user.Role.Name,
                }
            });
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = _user.UserId;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("User not found");

            var checkOldPassword = BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash);
            if (!checkOldPassword)
                return BadRequest("Old password is incorrect");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Change password successfully" });
        }
    }
}