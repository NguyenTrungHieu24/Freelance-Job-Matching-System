using BusinessObjects.DTOs;

namespace Client.Models.Freelancer;

public class FreelancerCvViewModel
{
    public FreelancerCvDto CvPortfolio { get; set; } = new();
    public List<SkillDTO> AllSkills { get; set; } = new();
    public List<int> SelectedSkill { get; set; } = new(); 
    public IFormFile? CvFile { get; set; }
}