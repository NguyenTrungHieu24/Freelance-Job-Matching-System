using BusinessObjects.DTOs;

namespace Client.Models.Freelancer;

public class FreelancerJobDetailViewModel
{
    public FreelancerJobDTO Job { get; set; } = new();
    public CreateApplicationDto ApplicationForm { get; set; } = new();
}
