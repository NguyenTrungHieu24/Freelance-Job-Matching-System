namespace Client.Models.Auth
{
    public class LoginResponse
    {
        public string Role { get; set; }
        public string Token { get; set; }

        public CurrentUser User { get; set; }
    }
}
