using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class ResetPasswordModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Token { get; set; } = string.Empty; // Mã bảo mật gửi qua email
        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
