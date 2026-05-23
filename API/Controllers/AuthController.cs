using API.Services;
using BusinessObjects;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
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

        public AuthController(AppDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        // =========================
        // REGISTER
        // =========================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var exists = await _context.Users
                .AnyAsync(x => x.Email == dto.Email);

            if (exists)
                return BadRequest("Email already exists");

            var account = new User
            {
                FullName = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = dto.Role ?? (int)RoleEnum.EMPLOYER
            };

            _context.Users.Add(account);
            await _context.SaveChangesAsync();

            var e = new EmployerProfile
            {
                AccountId = account.Id
            };

            _context.EmployerProfiles.Add(e);
            await _context.SaveChangesAsync();

            var token = _jwt.GenerateToken(account);

            return Ok(new
            {
                Token = token,
                Role = account.Role,
                User = new
                {
                    RunnerId = e.Id,
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
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

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
    }
}
