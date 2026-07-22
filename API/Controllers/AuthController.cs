using API.Services;
using API.Services.Auth;
using BusinessObjects;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly IUserService _user;
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context, JwtService jwt, IUserService user, IEmailService emailService)
        {
            _context = context;
            _jwt = jwt;
            _user = user;
            _emailService = emailService;
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
            EmployerProfile e = null;

            switch (account.RoleId)
            {
                case 3:
                    var f = new FreelancerProfile
                    {
                        AccountId = account.Id,
                        Title = "New Freelancer"
                    };
                    _context.FreelancerProfiles.Add(f);
                    await _context.SaveChangesAsync();
                    break;
                case 2:
                    e = new EmployerProfile
                    {
                        AccountId = account.Id,
                        CompanyName = account.FullName,
                        Description = "",
                        Logo = "default-logo.png"
                    };
                    _context.EmployerProfiles.Add(e);
                    await _context.SaveChangesAsync();
                    break;
            }

            // Tao vi ao cho Employer/Freelancer moi
            if (account.RoleId == (int)RoleEnum.EMPLOYER
                || account.RoleId == (int)RoleEnum.FREELANCER)
            {
                _context.Wallets.Add(new Wallet
                {
                    UserId = account.Id,
                    Balance = 0
                });
                await _context.SaveChangesAsync();
            }

            await _context.Entry(account)
                .Reference(x => x.Role)
                .LoadAsync();

            var jwt = _jwt.GenerateToken(account);

            return Ok(new
            {
                Token = jwt.Token,
                ExpiresAt = jwt.ExpiresAt,
                Role = account.Role.Name,
                User = new
                {
                    Id = account.Id,
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

            if (!user.IsActive)
                return Unauthorized("Tai khoan cua ban da bi khoa. Vui long lien he voi quan tri vien de biet them chi tiet!");

            await _context.Entry(user)
                .Reference(x => x.Role)
                .LoadAsync();

            var jwt = _jwt.GenerateToken(user);

            return Ok(new
            {
                Token = jwt.Token,
                ExpiresAt = jwt.ExpiresAt,
                Role = user.Role.Name,
                User = new
                {
                    Id = user.Id,
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

        // =========================
        // FORGOT PASSWORD (Yeu cau khoi phuc)
        // =========================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            
            // de tranh ke xau do tim email co ton tai trong he thong hay khong.
            if (user == null)
                return Ok(new { message = "Neu email ton tai tren he thong, ma khoi phuc da duoc gui." });

            // 1. Tao ma OTP 6 so de nguoi dung nhap tren trang dat lai mat khau
            var resetToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            // 2. Luu Token va thoi gian het han truc tiep vao bang User
           
            user.PasswordResetToken = resetToken;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(15); // Token co hieu luc trong 15 phut

            await _context.SaveChangesAsync();

            // 3. GUI EMAIL CHUA MA OTP
            var subject = "Khoi phuc mat khau";
            var body = $@"
        <p>Xin chao <b>{user.FullName}</b>,</p>
        <p>Ban vua yeu cau dat lai mat khau.</p>
        <p>Ma OTP dat lai mat khau cua ban la:</p>
        <h2 style='letter-spacing:4px'>{resetToken}</h2>
        <p>Ma nay co hieu luc trong 15 phut.</p>
        <p>Neu ban khong yeu cau, hay bo qua email nay.</p>
    ";

            await _emailService.SendEmailAsync(user.Email, subject, body);


            return Ok(new
            {
                message = "Ma khoi phuc da duoc tao thanh cong.",
                
            });
        }

        // Test endpoint to send an email via configured IEmailService
        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                await _emailService.SendEmailAsync(
                    "hieu81194@gmail.com",
                    "Test Mail",
                    "<h2>Gui thanh cong</h2>"
                );

                return Ok(new { message = "Email sent" });
            }
            catch (Exception ex)
            {
                // Return full exception for debugging; remove/limit in production
                return BadRequest(new { error = ex.ToString() });
            }
        }
// Removed duplicate constructor; single constructor now includes IEmailService.

        // =========================
        // RESET PASSWORD (Xac nhan doi mat khau moi)
        // =========================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return BadRequest("Yeu cau khong hop le.");

            // Kiem tra Token khop khong va da het han chua
            if (user.PasswordResetToken != dto.Token.Trim() || user.ResetTokenExpires < DateTime.UtcNow)
            {
                return BadRequest("Ma xac thuc khong chinh xac hoac da het han.");
            }

            // Tien hanh doi mat khau moi (Bcryt mat khau bang BCrypt tuong tu nhu luc Register)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            // Xoa token sau khi da su dung thanh cong de tranh dung lai
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Dat lai mat khau thanh cong! Ban co the dang nhap ngay bay gio." });
        }


    }
}
