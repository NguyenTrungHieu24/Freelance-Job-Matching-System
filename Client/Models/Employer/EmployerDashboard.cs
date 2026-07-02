using BusinessObjects.DTOs;

namespace Client.Models.Employer;

public class EmployerDashboard
{
    public int TotalJobs { get; set; }

    public int ActiveJobs { get; set; }

    public int TotalApplications { get; set; }

    public int PendingApplications { get; set; }

    public List<EmployerRecentJobDto> RecentJobs { get; set; } = [];

    public List<RecentApplicationDto> RecentApplications { get; set; } = [];
}