using BusinessObjects.Enums;

namespace BusinessObjects.DTOs;

public class EmployerDto
{
    public int Id  { get; set; }
    public int AccountId  { get; set; }
    public string FullName  { get; set; }
    public string CompanyName { get; set; }
    public string Description { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Logo { get; set; }
}

public class UpdateEmployerProfileDto
{
    public string FullName { get; set; }
    public string CompanyName { get; set; }
    public string Description { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public class RecentApplicationDto
{
    public int Id { get; set; }

    public string CandidateName { get; set; }

    public string JobTitle { get; set; }

    public DateTime AppliedAt { get; set; }

    public ApplicationStatus Status { get; set; }
}

public class EmployerRecentJobDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int ApplicationCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Location { get; set; }
    public List<string> Skills { get; set; } = [];
}

public class EmployerDashboardDto
{
    public int TotalJobs { get; set; }

    public int ActiveJobs { get; set; }

    public int TotalApplications { get; set; }

    public int PendingApplications { get; set; }

    public List<EmployerRecentJobDto> RecentJobs { get; set; } = [];

    public List<EmployerRecentApplicationDto> RecentApplications { get; set; } = [];
}
public class EmployerRecentApplicationDto
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public string JobTitle { get; set; } = "";

    public string CandidateName { get; set; } = "";

    public string CandidateAvatar { get; set; } = "";

    public DateTime AppliedAt { get; set; }

    public ApplicationStatus Status { get; set; }
}
