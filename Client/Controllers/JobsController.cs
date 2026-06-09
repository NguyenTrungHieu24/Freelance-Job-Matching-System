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
        public IActionResult Index()
        {
            return View();
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

        private static List<KeyValuePair<string, string>> BuildQueryParams(FilterJobDTO filter)
        {
            var queryParams = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                queryParams.Add(new KeyValuePair<string, string>("keyword", filter.Keyword));

            if (!string.IsNullOrWhiteSpace(filter.EmployerKeyword))
                queryParams.Add(new KeyValuePair<string, string>("employerKeyword", filter.EmployerKeyword));

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

        [HttpGet("details/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var job = await GetAsync<JobDTO>($"api/jobs/{id}");
                if (job == null)
                {
                    return NotFound();
                }
                return View(job);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Cannot load job details: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost("apply/{id:int}")]
        [Authorize(Roles = "FREELANCER")]
        public async Task<IActionResult> Apply(int id, [FromForm] string coverLetter)
        {
            try
            {
                var response = await PostAsync<object, dynamic>($"api/jobs/{id}/apply", new { CoverLetter = coverLetter });
                TempData["Success"] = "Ứng tuyển dự án thành công!";
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(ex.Message);
                    if (doc.RootElement.TryGetProperty("message", out var msgProp))
                    {
                        errMsg = msgProp.GetString() ?? ex.Message;
                    }
                }
                catch
                {
                    // Fallback to raw message
                }
                TempData["Error"] = $"Ứng tuyển thất bại: {errMsg}";
            }
            return RedirectToAction("Details", new { id = id });
        }
    }
}
