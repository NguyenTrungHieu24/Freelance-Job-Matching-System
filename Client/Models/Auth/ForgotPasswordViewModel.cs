using System.ComponentModel.DataAnnotations;

namespace Client.Models.Auth 
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;
    }
}