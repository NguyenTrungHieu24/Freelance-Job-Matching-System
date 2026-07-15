using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Models;
using Client.Models.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Client.Controllers
{
    [Route("jobs")]
    public class JobsController : BaseController
    {
        public JobsController(IHttpClientFactory factory) : base(factory)
        {
        }

        [Route("")]
        public async Task<IActionResult> Index(FilterJobDTO filter, [FromQuery] int? page)
        {
            try
            {
                if (page.HasValue)
                {
                    filter.Page = page.Value;
                }
                filter.Page = filter.Page <= 0 ? 1 : filter.Page;

                var queries = BuildQueryParams(filter);
                var url = QueryHelpers.AddQueryString("api/jobs", queries);

                var jobsTask = GetAsync<PaginateResult<JobDTO>>(url);
                var skillsTask = GetAsync<List<SkillDTO>>("api/skills/all");
                var categoriesTask = GetAsync<List<CategoryDTO>>("api/categories");

                await Task.WhenAll(jobsTask, skillsTask, categoriesTask);

                ViewBag.Category = categoriesTask.Result;
                ViewBag.Skills = skillsTask.Result;

                return View(new ListJobsModel
                {
                    Filter = filter,
                    Jobs = jobsTask.Result ?? new PaginateResult<JobDTO>()
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


        [Route("manage")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Manage(FilterJobDTO filter, [FromQuery] int? page)
        {
            try
            {
                if (page != null)
                {
                    filter.Page = (int)page;
                }

                var queries = BuildQueryParams(filter);


                var url = QueryHelpers.AddQueryString("api/jobs", queries);

                // Lay jobs
                var data = await GetAsync<PaginateResult<JobDTO>>(url);

                var skills = await GetAsync<List<SkillDTO>>("api/skills/all");

                ViewBag.Skills = new SelectList(skills, "Id", "Name");

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

        [Route("detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var job = await GetAsync<JobDto>($"api/jobs/{id}");
                if (job == null)
                {
                    TempData["Error"] = "Job not found.";
                    return RedirectToAction("Index", "Home");
                }
                return View(job);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Cannot load job details: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        private static List<KeyValuePair<string, string>> BuildQueryParams(FilterJobDTO filter)
        {
            var queryParams = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                queryParams.Add(new KeyValuePair<string, string>("keyword", filter.Keyword.Trim()));

            if (!string.IsNullOrWhiteSpace(filter.EmployerKeyword))
                queryParams.Add(new KeyValuePair<string, string>("employerKeyword", filter.EmployerKeyword.Trim()));

            if (filter.Status.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("status", ((int)filter.Status.Value).ToString()));

            if (filter.CategoryId.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("categoryId", filter.CategoryId.ToString()));

            if (filter.EmployerProfileId.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("employerProfileId", filter.EmployerProfileId.ToString()));

            if (filter.MinBudget.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("minBudget", filter.MinBudget.ToString()));

            if (filter.MaxBudget.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("maxBudget", filter.MaxBudget.ToString()));

            if (filter.CreatedFrom.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("createdFrom", filter.CreatedFrom.Value.ToString("yyyy-MM-dd")));

            if (filter.Temperature.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("temperature", filter.Temperature.Value.ToString()));

            if (filter.CreatedTo.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("createdTo", filter.CreatedTo.Value.ToString("yyyy-MM-dd")));

            if (filter.DeadlineFrom.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("deadlineFrom", filter.DeadlineFrom.Value.ToString("yyyy-MM-dd")));

            if (filter.DeadlineTo.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("deadlineTo", filter.DeadlineTo.Value.ToString("yyyy-MM-dd")));

            if (filter.SkillIds.Count > 0)
            {
                foreach (var skillId in filter.SkillIds)
                {
                    queryParams.Add(new KeyValuePair<string, string>(
                       "skillIds",
                       skillId.ToString())
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
                queryParams.Add(new KeyValuePair<string, string>("sortBy", filter.SortBy));

            queryParams.Add(new KeyValuePair<string, string>("isDescending", filter.IsDescending.ToString()));
            queryParams.Add(new KeyValuePair<string, string>("page", (filter.Page == 0 ? 1 : filter.Page).ToString()));
            queryParams.Add(new KeyValuePair<string, string>("pageSize", filter.PageSize.ToString()));

            return queryParams;
        }

        [HttpPost]
        [Route("close")]
        public async Task<IActionResult> CloseJob([FromQuery] int id)
        {
            try
            {
                var result = await PostAsync<string, ApiResult<bool>>($"api/jobs/close/{id}", null);

                if (!result.Success)
                {
                    return StatusCode(500, result);
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<bool>.Fail(ex.Message));
            }
        }


        [HttpPost]
        [Route("open")]
        public async Task<IActionResult> OpenJob([FromQuery] int id)
        {
            try
            {
                var result = await PostAsync<string, ApiResult<bool>>($"api/jobs/open/{id}", null);

                if (!result.Success)
                {
                    return StatusCode(500, result);
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<bool>.Fail(ex.Message));
            }
        }
    }
}
