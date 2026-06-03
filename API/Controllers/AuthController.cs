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

            await _context.Entry(account)
                .Reference(x => x.Role)
                .LoadAsync();

            var token = _jwt.GenerateToken(account);



            return Ok(new
            {
                Token = token,
                Role = account.Role.Name,
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

            var jwt = _jwt.GenerateToken(user);

            return Ok(new
            {
                Token = jwt.Token,
                ExpiresAt = jwt.ExpiresAt,
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

        // =========================
        // FORGOT PASSWORD (Yêu cầu khôi phục)
        // =========================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            
            // để tránh kẻ xấu dò tìm email có tồn tại trong hệ thống hay không.
            if (user == null)
                return Ok(new { message = "Nếu email tồn tại trên hệ thống, mã khôi phục đã được gửi." });

            // 1. Tạo mã OTP 6 số để người dùng nhập trên trang đặt lại mật khẩu
            var resetToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            // 2. Lưu Token và thời gian hết hạn trực tiếp vào bảng User
           
            user.PasswordResetToken = resetToken;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(15); // Token có hiệu lực trong 15 phút

            await _context.SaveChangesAsync();

            // 3. GỬI EMAIL CHỨA MÃ OTP
            var subject = "Khôi phục mật khẩu";
            var body = $@"
        <p>Xin chào <b>{user.FullName}</b>,</p>
        <p>Bạn vừa yêu cầu đặt lại mật khẩu.</p>
        <p>Mã OTP đặt lại mật khẩu của bạn là:</p>
        <h2 style='letter-spacing:4px'>{resetToken}</h2>
        <p>Mã này có hiệu lực trong 15 phút.</p>
        <p>Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
    ";

            await _emailService.SendEmailAsync(user.Email, subject, body);


            return Ok(new
            {
                message = "Mã khôi phục đã được tạo thành công.",
                
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
                    "<h2>Gửi thành công</h2>"
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
        // RESET PASSWORD (Xác nhận đổi mật khẩu mới)
        // =========================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return BadRequest("Yêu cầu không hợp lệ.");

            // Kiểm tra Token khớp không và đã hết hạn chưa
            if (user.PasswordResetToken != dto.Token.Trim() || user.ResetTokenExpires < DateTime.UtcNow)
            {
                return BadRequest("Mã xác thực không chính xác hoặc đã hết hạn.");
            }

            // Tiến hành đổi mật khẩu mới (Bcryt mật khẩu bằng BCrypt tương tự như lúc Register)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            // Xóa token sau khi đã sử dụng thành công để tránh dùng lại
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập ngay bây giờ." });
        }


    }
}
