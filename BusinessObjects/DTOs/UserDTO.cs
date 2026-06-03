using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; }
    }

    public class UpdateAccountDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid account Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email format")]
        [StringLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Role is required")]

        [RegularExpression("", ErrorMessage = "Invalid Role")]

        public string Role { get; set; }
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email format")]
        [StringLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }



        [Range(1, int.MaxValue, ErrorMessage = "Invalid Role")]
        public int? Role { get; set; }
        public string? CompanyName { get; set; }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Enter old password")]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "Enter new password")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;
    }


    public class FilterUserDTO
    {
        public string? Keyword { get; set; }

        public int? Status { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public List<int> RoleIds { get; set; } = [];
        public string? SortBy { get; set; } = "CreatedAt";
        public bool IsDescending { get; set; } = true;

        public int Page { get; set; }

        public int PageSize { get; set; } = 10;
    }
}
