using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Http;

namespace Client.Models.Freelancer;

public class FreelancerJobDetailViewModel
{
    public FreelancerJobDTO Job { get; set; } = new();
    public CreateApplicationDto ApplicationForm { get; set; } = new();
    public IFormFile? CvFile { get; set; }
}
