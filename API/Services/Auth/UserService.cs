using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Services.Auth
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated =>
            User?.Identity?.IsAuthenticated ?? false;
        
        public int UserId =>
            int.Parse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        public string Role =>
            User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
    }
}
