using System.ComponentModel.DataAnnotations;

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