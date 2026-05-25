using BusinessObjects.DTOs;

namespace Client.Models.Freelancer;

public class FreelancerPersonalInfoViewModel
{
    public FreelancerPersonalInfoDto PersonalInfo { get; set; } = new();
    public IFormFile? ProfilePhoto { get; set; }    
}