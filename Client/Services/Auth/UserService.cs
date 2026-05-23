using Client.Models.Auth;
using System.Text.Json;

namespace Client.Services.Auth
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _http;

        public UserService(IHttpContextAccessor http)
        {
            _http = http;
        }

        private ISession Session => _http.HttpContext!.Session;

        public string? Jwt => Session.GetString("Auth.JWT");

        public string? Role => Session.GetString("Auth.Role");

        public bool IsAuthenticated => !string.IsNullOrEmpty(Jwt);

        public CurrentUser? Info
        {
            get
            {
                var json = Session.GetString("Auth.User");
                return string.IsNullOrEmpty(json)
                    ? null
                    : JsonSerializer.Deserialize<CurrentUser>(json);
            }
        }

        public bool IsAdmin => Role == "Admin";
        public bool IsRunner => Role == "Runner";
    }
}
