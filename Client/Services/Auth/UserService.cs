using System.Security.Claims;
using BusinessObjects.Enums;
using Client.Models.Auth;

namespace Client.Services.Auth
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _http;

        public UserService(IHttpContextAccessor http)
        {
            _http = http;
        }

        private ClaimsPrincipal? User => _http.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;

        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

        public string? Name => User?.FindFirst(ClaimTypes.Name)?.Value;

        public bool IsAdmin => User?.IsInRole(nameof(RoleEnum.ADMIN)) ?? false;

        public bool IsFreelancer => User?.IsInRole(nameof(RoleEnum.FREELANCER)) ?? false;

        public bool IsEmployer => User?.IsInRole(nameof(RoleEnum.EMPLOYER)) ?? false;

        public bool IsGuest => !IsAuthenticated;
    }
}