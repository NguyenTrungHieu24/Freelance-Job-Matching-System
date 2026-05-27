using PayOS;

namespace API.Configurations
{
    public class PayOSSettings : PayOSOptions
    {
        public string ReturnUrl { get; set; } = string.Empty;

        public string CancelUrl { get; set; } = string.Empty;
    }
}
