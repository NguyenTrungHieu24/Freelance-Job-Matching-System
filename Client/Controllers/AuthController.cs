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

        public IActionResult ForgotPassword()
        {
            return View();
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
    }
}