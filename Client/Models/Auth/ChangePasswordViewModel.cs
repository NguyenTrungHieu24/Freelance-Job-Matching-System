using System.ComponentModel.DataAnnotations;

namespace Client.Models.Auth;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Current password is required.")]
    public string OldPassword { get; set; } = null!;

    [Required(ErrorMessage = "New password is required.")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Confirm new password is required.")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}