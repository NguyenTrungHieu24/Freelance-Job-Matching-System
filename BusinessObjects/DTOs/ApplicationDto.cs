using System.ComponentModel.DataAnnotations;
using BusinessObjects.Enums;

namespace BusinessObjects.DTOs;

public class ApplicationDto
{
    
}

public class CreateApplicationDto
{
    [Required]
    public int JobId { get; set; }
    [Required(ErrorMessage = "Cover letter is required")]
    [StringLength(2000, ErrorMessage =  "Cover letter is too long")]
    public string CoverLetter { get; set; }
}

public class ApplicationHistoryDto
{
    public int Id { get; set; }
    public int JobId  { get; set; }
    public string JobTitle { get; set; }
    public decimal Budget { get; set; }
    public DateTime? Deadline  { get; set; }
    public string EmployerName  { get; set; }
    public string CompanyName   { get; set; }
    public string Logo { get; set; }
    public string CoverLetter { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAt  { get; set; }
}