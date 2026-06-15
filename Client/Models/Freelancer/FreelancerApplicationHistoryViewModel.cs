using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;

namespace Client.Models.Freelancer;

public class FreelancerApplicationHistoryViewModel
{
    public PaginateResult<ApplicationHistoryDto> Applications  { get; set; }
    public ApplicationStatus Status { get; set; }
}