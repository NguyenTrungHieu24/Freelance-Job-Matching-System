using System.ComponentModel.DataAnnotations;
using BusinessObjects.Enums;

namespace BusinessObjects.DTOs;

public class FreelancerPersonalInfoDto
{
    public int AccountId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    [StringLength(12)]
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ProfilePhoto { get; set; } 
}

public class UpdateFreelancerPersonalInfoDto
{
    [StringLength(100)]
    public string FullName { get; set; } = null!;
    [EmailAddress(ErrorMessage = "Invalid Email address")]
    public string Email { get; set; } = null!;
    [Phone(ErrorMessage = "Invalid phone number")]
    [StringLength(12)]
    public string? Phone { get; set; }
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

public class FreelancerJobDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Budget { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EmployerProfileId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public string EmployerName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string EmployerLogo { get; set; } = string.Empty;
    public int PostedJobCount { get; set; } = 0;
    public bool IsApplied { get; set; } = false;
    public ApplicationStatus? ApplyStatus { get; set; }
    public int ApplicationsCount { get; set; } = 0;
}

public class FreelancerFilterJobDTO
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public List<int> SkillIds { get; set; } = new();
    public JobStatus? Status { get; set; }
    public decimal? MinBudget { get; set; }
    public decimal? MaxBudget { get; set; }
    public DateTime? DeadlineFrom { get; set; }
    public DateTime? DeadlineTo { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public JobTemperature? Temperature { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool IsDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class FreelancerDashboardDto
{
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int CancelledApplications { get; set; }
    public decimal TotalEarnings { get; set; }

    public List<ApplicationHistoryDto> RecentApplications { get; set; }
    public List<FreelancerJobDTO> RecommendedJobs { get; set; }
}
public class MyJobDto
{
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } 
    public string EmployerName { get; set; } 
    public int EmployerId { get; set; }
    public string CompanyName  { get; set; }
    public decimal Budget { get; set; }
    public DateTime AppliedAt { get; set; }
    
    public ApplicationStatus Status { get; set; }
    public JobStatus JobStatus { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public bool IsReviewed { get; set; } = false;
    
    public int ProgressStage { get; set; }
}
