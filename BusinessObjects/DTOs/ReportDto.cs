using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs;

public class ReportDto
{
    
}

public class CreateReportDto
{
    [Required]
    public int ReportUserId { get; set; }
    [Required(ErrorMessage = "Please enter a reason for the report")]
    [MaxLength(255)]
    public string Reason { get; set; }
    public string? Description { get; set; }
    
}