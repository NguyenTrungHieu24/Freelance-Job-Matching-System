using System.Text.Json;

namespace Client.Models.Auth
{
    public class CurrentUserViewModel
    {
        public string? Jwt { get; set; }
        public string? Role { get; set; }

        public CurrentUser? Info { get; set; }

        public bool IsAuthenticated => !string.IsNullOrEmpty(Jwt);
    }
}
