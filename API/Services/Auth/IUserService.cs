namespace API.Services.Auth
{
    public interface IUserService
    {
        int UserId { get; }
        string Role { get; }
        bool IsAuthenticated { get; }
    }
}
