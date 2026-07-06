using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;

namespace Client.Models.Admin;

public class ReportViewModel
{
    public PaginateResult<ReportDto> Reports { get; set; }
    public ReportStatus  Status { get; set; }
}