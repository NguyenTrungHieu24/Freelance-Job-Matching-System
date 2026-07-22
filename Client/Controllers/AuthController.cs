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

                await HttpContext.SignInAsync("Cookies", principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = result.ExpiresAt
                });

                Response.Cookies.Append(
                    "Auth.JWT",
                    result.Token,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = result.ExpiresAt
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
                else if (result.Role.Equals("EMPLOYER"))
                    return RedirectToAction("Dashboard", "Employer");
                else
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                ViewBag.Error = ParseErrorMessage(e.Message);
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
                ViewBag.Error = "Mat khau xac nhan khong khop.";
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
                    ViewBag.Error = "Dang ky khong thanh cong. Vui long thu lai!";
                    return View(model);
                }

                TempData["Success"] = "Dang ky tai khoan thanh cong! Hay dang nhap de tiep tuc!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ParseErrorMessage(ex.Message);
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

        // 1. GET: Hien thi trang nhap Email quen mat khau
        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // 2. POST: Gui email len API
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Su dung ham PostAsync co san cua BaseController de goi sang API
                // "api/auth/forgot-password" la endpoint tuong ung ben phia du an API
                var result = await PostAsync<ForgotPasswordViewModel, object>(
                    "api/auth/forgot-password",
                    model
                );

                TempData["SuccessMessage"] = "Neu email ton tai, he thong da gui ma OTP dat lai mat khau.";
                return RedirectToAction("ResetPassword", new { email = model.Email });
            }
            catch (Exception ex)
            {
                // Neu API tra ve BadRequest hoac loi, no se nhay vao day
                ModelState.AddModelError("", ex.Message ?? "Co loi xay ra tu he thong. Vui long thu lai.");
                return View(model);
            }
        }

        // ==========================================
        // RESET PASSWORD
        // ==========================================

        // 3. GET: Hien thi trang nhap mat khau moi
        // Duong dan thuc te se co dang: /auth/reset-password?email=abc@gmail.com&token=xyz
        [HttpGet("reset-password")]
        public IActionResult ResetPassword([FromQuery] string email, [FromQuery] string? token)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Email = email, Token = token ?? string.Empty };
            return View(model);
        }

        // 4. POST: Gui mat khau moi len API de cap nhat du lieu
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Goi sang endpoint reset-password cua API bang ham ke thua tu BaseController
                var result = await PostAsync<ResetPasswordViewModel, object>(
                    "api/auth/reset-password",
                    model
                );

                // Chuyen huong ve trang Login kem thong bao thanh cong hien thi cho nguoi dung
                TempData["SuccessMessage"] = "Dat lai mat khau thanh cong! Vui long dang nhap bang mat khau moi.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Hien thi loi chi tiet tu API tra ve (vi du: "Ma xac thuc da het han")
                ModelState.AddModelError("", ex.Message ?? "Ma xac thuc khong hop le hoac da het han.");
                return View(model);
            }
        }


        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                // Call API endpoint to send email from server-side
                var result = await PostAsync<object, object>(
                    "api/auth/test-email",
                    new { }
                );

                return Ok("Requested send mail via API");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

    }
}
