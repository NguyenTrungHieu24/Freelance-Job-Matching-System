using Client.Models.Auth;
using System.Security.Claims;

namespace Client.Services.Auth
{
    public interface IUserService
    {
        bool IsAuthenticated { get; }
        string? Role { get; }
        string? Name { get; }
        string? Email { get; }
        bool IsAdmin { get; }
        bool IsFreelancer { get; }
        bool IsEmployer { get; }
        bool IsGuest { get; }
        bool IsFinanceManager { get; }
    }
}
