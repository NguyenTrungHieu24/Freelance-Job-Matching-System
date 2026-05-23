using Client.Models.Auth;

namespace Client.Services.Auth
{
    public interface IUserService
    {
        bool IsAuthenticated { get; }
        string? Role { get; }
        CurrentUser? Info { get; }

        bool IsAdmin { get; }
        bool IsFreelancer { get; }
        bool IsEmployer { get; }
        bool IsGuest { get; }
    }
}
