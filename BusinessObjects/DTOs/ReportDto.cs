using System.ComponentModel.DataAnnotations;
using BusinessObjects.Enums;

namespace BusinessObjects.DTOs;

public class ReportDto
{
    public int Id  { get; set; }
    public int ReporterId  { get; set; }
    public string ReporterName  { get; set; }
    public int ReportedUserId  { get; set; }
    public string ReportedUserName  { get; set; }
    public string Reason { get; set; }
    public string? Description  { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedOn  { get; set; }
    public DateTime? ResolvedAt  { get; set; }
    public int? ResolverId  { get; set; }
    public string? ResolverName  { get; set; }
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