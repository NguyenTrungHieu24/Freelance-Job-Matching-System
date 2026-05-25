using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs;

public class FreelancerPersonalInfoDto
{
    public int AccountId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ProfilePhoto { get; set; } 
}

public class UpdateFreelancerPersonalInfoDto
{
    [Required(ErrorMessage = "Full name must be provided")]
    [StringLength(100)]
    public string FullName { get; set; } = null!;
    [Required(ErrorMessage = "Email must be required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = null!;
    [Required(ErrorMessage = "Phone number must be required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    [StringLength(20)]
    public string? Phone { get; set; }
    [Required(ErrorMessage = "Address must be required")]
    [StringLength(255)]
    public string? Address { get; set; }
}

public class FreelancerCvDto
{
    public int ProfileId { get; set; }
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string? CVUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? PortfolioDescription { get; set; }
    public List<SkillDTO> Skills { get; set; } = new();
}
public class UpdateFreelancerCvDto
{
    [StringLength(255)]
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string? CVUrl { get; set; }
    [Url(ErrorMessage = "Đường dẫn Portfolio không hợp lệ")]
    [StringLength(255)]
    public string? PortfolioUrl { get; set; }
    public string? PortfolioDescription { get; set; }
    public List<int> Skills { get; set; } = new();
}