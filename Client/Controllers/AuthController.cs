using Client.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Client.Controllers
{
    [Route("auth")]
    public class AuthController : BaseController
    {
        public AuthController(IHttpClientFactory factory)
            : base(factory)
        {
        }


        [HttpGet("login")]
        public IActionResult Login([FromQuery] string? path)
        {
            ViewData["Path"] = path;
            return View();
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await PostAsync<LoginViewModel, LoginResponse>(
                    "api/auth/login",
                    model
                );

                if (result == null)
                {
                    ViewBag.Error = "Invalid credentials.";
                    return View(model);
                }

                //SaveAuthSession(result);

                var claims = new List<Claim>{
                    new Claim(ClaimTypes.Name, result.User.Name),
                    new Claim(ClaimTypes.Email, result.User.Email),
                    new Claim(ClaimTypes.Role, result.Role),
                    new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString())
                };

                var identity = new ClaimsIdentity(claims, "Cookies");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("Cookies", principal);

                Response.Cookies.Append(
                    "Auth.JWT",
                    result.Token,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });

                // Safe redirect
                if (!string.IsNullOrEmpty(model.Path))
                {
                    try
                    {
                        var decoded = Encoding.UTF8.GetString(
                            Convert.FromBase64String(model.Path)
                        );

                        if (Url.IsLocalUrl(decoded))
                            return Redirect(decoded);
                    }
                    catch
                    {
                        // ignore invalid base64
                    }
                }
                
                if (result.Role.Equals("FREELANCER"))
                    return RedirectToAction("Dashboard", "Freelancer");
                else if(result.Role.Equals("EMPLOYER"))
                    return RedirectToAction("Dashboard", "Employer");
                else
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return View(model);
            }
        }


        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Password not match.";
                return View(model);
            }

            try
            {
                var result = await PostAsync<RegisterViewModel, LoginResponse>(
                    "api/auth/register",
                    model
                );

                if (result == null)
                {
                    ViewBag.Error = "Register failed.";
                    return View(model);
                }

                // Auto login
                //SaveAuthSession(result);

                if (result.Role.Equals("FREELANCER"))
                    return RedirectToAction("Dashboard", "Freelancer");
                else
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");

            Response.Cookies.Delete("Auth.JWT");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }


        private void SaveAuthSession(LoginResponse result)
        {
            HttpContext.Session.SetString("Auth.JWT", result.Token);
            HttpContext.Session.SetString("Auth.Role", result.Role);
            HttpContext.Session.SetString(
                "Auth.User",
                JsonSerializer.Serialize(result.User)
            );
        }

        // ==========================================
        // FORGOT PASSWORD
        // ==========================================

        // 1. GET: Hiển thị trang nhập Email quên mật khẩu
        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // 2. POST: Gửi email lên API
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Sử dụng hàm PostAsync có sẵn của BaseController để gọi sang API
                // "api/auth/forgot-password" là endpoint tương ứng bên phía dự án API
                var result = await PostAsync<ForgotPasswordViewModel, object>(
                    "api/auth/forgot-password",
                    model
                );

                ViewBag.SuccessMessage = "Yêu cầu thành công! Nếu email tồn tại, hệ thống đã gửi hướng dẫn đổi mật khẩu.";
                return View();
            }
            catch (Exception ex)
            {
                // Nếu API trả về BadRequest hoặc lỗi, nó sẽ nhảy vào đây
                ModelState.AddModelError("", ex.Message ?? "Có lỗi xảy ra từ hệ thống. Vui lòng thử lại.");
                return View(model);
            }
        }

        // ==========================================
        // RESET PASSWORD
        // ==========================================

        // 3. GET: Hiển thị trang nhập mật khẩu mới
        // Đường dẫn thực tế sẽ có dạng: /auth/reset-password?email=abc@gmail.com&token=xyz
        [HttpGet("reset-password")]
        public IActionResult ResetPassword([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Email = email, Token = token };
            return View(model);
        }

        // 4. POST: Gửi mật khẩu mới lên API để cập nhật dữ liệu
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Gọi sang endpoint reset-password của API bằng hàm kế thừa từ BaseController
                var result = await PostAsync<ResetPasswordViewModel, object>(
                    "api/auth/reset-password",
                    model
                );

                // Chuyển hướng về trang Login kèm thông báo thành công hiển thị cho người dùng
                TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi chi tiết từ API trả về (ví dụ: "Mã xác thực đã hết hạn")
                ModelState.AddModelError("", ex.Message ?? "Mã xác thực không hợp lệ hoặc đã hết hạn.");
                return View(model);
            }
        }

    }
}