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

            var e = new EmployerProfile
            {
                AccountId = account.Id
            };

            _context.EmployerProfiles.Add(e);
            await _context.SaveChangesAsync();

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
            
            if(user == null)
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

            // Vì lý do bảo mật, nếu không tìm thấy email, chúng ta vẫn trả về Ok 
            // để tránh kẻ xấu dò tìm email có tồn tại trong hệ thống hay không.
            if (user == null)
                return Ok(new { message = "Nếu email tồn tại trên hệ thống, mã khôi phục đã được gửi." });

            // 1. Tạo một Token ngẫu nhiên (hoặc mã OTP số tùy bạn chọn)
            // Ở đây dùng GUID để làm token trên URL cho an toàn
            var resetToken = Guid.NewGuid().ToString();

            // 2. Lưu Token và thời gian hết hạn trực tiếp vào bảng User
            // Lưu ý: Để chạy được 3 dòng dưới, bạn cần tạo thêm 2 cột: PasswordResetToken (string?) và ResetTokenExpires (DateTime?) vào class User trong DB.
            user.PasswordResetToken = resetToken;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(15); // Token có hiệu lực trong 15 phút

            await _context.SaveChangesAsync();

            // 3. GỬI EMAIL CHỨA LINK RESET
            // Tạm thời bạn có thể trả về Token này trong API để test bằng Postman/Swagger trước.
            // Khi làm thật, bạn sẽ viết dịch vụ gửi Email chứa link: https://localhost:CLIENT_PORT/Auth/ResetPassword?email={email}&token={resetToken}

            return Ok(new
            {
                message = "Mã khôi phục đã được tạo thành công.",
                debugToken = resetToken // Xóa dòng debugToken này khi deploy thực tế nhé!
            });
        }

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
            if (user.PasswordResetToken != dto.Token || user.ResetTokenExpires < DateTime.UtcNow)
            {
                return BadRequest("Mã xác thực không chính xác hoặc đã hết hạn.");
            }

            // Tiến hành đổi mật khẩu mới (Băm mật khẩu bằng BCrypt tương tự như lúc Register)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            // Xóa token sau khi đã sử dụng thành công để tránh dùng lại
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập ngay bây giờ." });
        }

    }
}
