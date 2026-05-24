using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs;

public class FreelancerDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string Title { get; set; }
    public string Bio { get; set; }
    public string? ProfilePhoto { get; set; }
    public string? CVUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? PortfolioDescription { get; set; }
    public List<SkillDTO> Skills { get; set; } = new();
}

public class UpdateFreelancerProfileDto
{
    [Required]
    [StringLength(255)]
    public string FullName { get; set; }
    [Required]
    [StringLength(255)]
    public string Email { get; set; }
    [Required]
    [StringLength(12)]
    public string PhoneNumber { get; set; }
    [Required]
    [StringLength(255)]
    public string Address { get; set; }
    [Required]
    [StringLength(255)]
    public string Title { get; set; }
    public string Bio { get; set; }
    public string? ProfilePhoto { get; set; }
    public string? CVUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? PortfolioDescription { get; set; }
    public List<SkillDTO> Skills { get; set; } = new();
}