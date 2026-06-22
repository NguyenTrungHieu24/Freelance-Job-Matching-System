using BusinessObjects.Common;
using BusinessObjects.DTOs;

namespace Client.Models.Freelancer;

public class FreelancerListJobsModel
{
    public FreelancerFilterJobDTO Filter { get; set; } = new();
    public PaginateResult<FreelancerJobDTO> Jobs  { get; set; } = new();
}