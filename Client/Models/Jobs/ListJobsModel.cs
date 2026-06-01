using BusinessObjects.Common;
using BusinessObjects.DTOs;

namespace Client.Models.Jobs
{
    public class ListJobsModel
    {
        public FilterJobDTO Filter { get; set; } = new();
        public PaginateResult<JobDTO> Jobs { get; set; }

        public bool HasFilter()
        {
            return
                !string.IsNullOrWhiteSpace(Filter.Keyword)
                || !string.IsNullOrWhiteSpace(Filter.EmployerKeyword)
                || Filter.Status.HasValue
                || Filter.MinBudget.HasValue
                || Filter.MaxBudget.HasValue
                || Filter.CreatedFrom.HasValue
                || Filter.CreatedTo.HasValue
                || Filter.Temperature.HasValue;
        }
    }
}
