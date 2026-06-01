using BusinessObjects.DTOs;

namespace Client.Models.Employer;

public class EmployerPersonalInfoViewModel
{
    public EmployerDto Employer { get; set; } = new();
    public IFormFile LogoImg { get; set; }
}