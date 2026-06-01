using BusinessObjects.Common;
using BusinessObjects.DTOs;
using Client.Models.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Client.Controllers
{
    [Route("jobs")]
    public class JobsController : BaseController
    {
        public JobsController(IHttpClientFactory factory) : base(factory)
        {
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }


        [Route("manage")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Manage(FilterJobDTO filter)
        {
            try
            {
                var url = QueryHelpers.AddQueryString(
                    "api/jobs",
                    BuildQueryParams(filter));

                var data = await GetAsync<PaginateResult<JobDTO>>(url);

                return View(new ListJobsModel
                {
                    Filter = filter,
                    Jobs = data ?? new PaginateResult<JobDTO>()
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Cannot load jobs: {ex.Message}";

                return View(new ListJobsModel
                {
                    Filter = filter,
                    Jobs = new PaginateResult<JobDTO>()
                });
            }
        }

        private static Dictionary<string, string?> BuildQueryParams(FilterJobDTO filter)
        {
            var queryParams = new Dictionary<string, string?>();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                queryParams["keyword"] = filter.Keyword;

            if (!string.IsNullOrWhiteSpace(filter.EmployerKeyword))
                queryParams["employerKeyword"] = filter.EmployerKeyword;

            if (filter.Status.HasValue)
                queryParams["status"] = ((int)filter.Status.Value).ToString();

            if (filter.CategoryId.HasValue)
                queryParams["categoryId"] = filter.CategoryId.ToString();

            if (filter.EmployerProfileId.HasValue)
                queryParams["employerProfileId"] = filter.EmployerProfileId.ToString();

            if (filter.MinBudget.HasValue)
                queryParams["minBudget"] = filter.MinBudget.ToString();

            if (filter.MaxBudget.HasValue)
                queryParams["maxBudget"] = filter.MaxBudget.ToString();

            if (filter.CreatedFrom.HasValue)
                queryParams["createdFrom"] = filter.CreatedFrom.Value.ToString("yyyy-MM-dd");

            if (filter.Temperature.HasValue)
                queryParams["temperature"] = filter.Temperature.Value.ToString();

            if (filter.CreatedTo.HasValue)
                queryParams["createdTo"] = filter.CreatedTo.Value.ToString("yyyy-MM-dd");

            if (filter.DeadlineFrom.HasValue)
                queryParams["deadlineFrom"] = filter.DeadlineFrom.Value.ToString("yyyy-MM-dd");

            if (filter.DeadlineTo.HasValue)
                queryParams["deadlineTo"] = filter.DeadlineTo.Value.ToString("yyyy-MM-dd");

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
                queryParams["sortBy"] = filter.SortBy;

            queryParams["isDescending"] = filter.IsDescending.ToString();
            queryParams["page"] = filter.Page.ToString();
            queryParams["pageSize"] = filter.PageSize.ToString();

            return queryParams;
        }
    }
}
