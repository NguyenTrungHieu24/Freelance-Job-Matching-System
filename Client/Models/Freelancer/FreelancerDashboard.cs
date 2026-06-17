using BusinessObjects.DTOs;

namespace Client.Models.Freelancer;

public class FreelancerDashboard
{
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int CancelledApplications { get; set; }
    public decimal TotalEarnings { get; set; }

    public List<ApplicationHistoryDto> RecentApplications { get; set; } = new();
    public List<FreelancerJobDTO> RecommendedJobs { get; set; } = new();
}