using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Client.Models.Admin;
using Client.Models.Skills;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Client.Controllers
{
    [Route("admin")]
    public class AdminController : BaseController
    {
        public AdminController(IHttpClientFactory factory) : base(factory)
        {
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet("dashboard/overview")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetOverview(DashboardRangeType range)
        {
            var userStatsTask = GetAsync<UserStats>($"api/admin/dashboard/user-stats?range={range}");

            var jobStatsTask = GetAsync<JobStats>($"api/admin/dashboard/job-stats?range={range}");

            var applicationStatsTask = GetAsync<ApplicationStats>($"api/admin/dashboard/application-stats?range={range}");

            var revenueStatsTask = GetAsync<RevenueStats>($"api/admin/dashboard/revenue-stats?range={range}");

            await Task.WhenAll(
                userStatsTask,
                jobStatsTask,
                applicationStatsTask,
                revenueStatsTask
            );

            var result = new OverviewStats
            {
                UserStats = await userStatsTask,
                JobStats = await jobStatsTask,
                ApplicationStats = await applicationStatsTask,
                RevenueStats = await revenueStatsTask
            };

            return Json(result);
        }

        [HttpGet("dashboard/user-growth")]
        public async Task<IActionResult> UserGrowth(DashboardRangeType range)
        {
            var data = await GetAsync<List<UserGrowthChartItem>>($"api/admin/dashboard/user-growth?range={range}");

            return Json(data);
        }

        [HttpGet("dashboard/job-growth")]
        public async Task<IActionResult> JobGrowth(DashboardRangeType range)
        {
            var data = await GetAsync<List<JobGrowthChartItem>>($"api/admin/dashboard/job-growth?range={range}");

            return Json(data);
        }

        [HttpGet("dashboard/recent-users")]
        public async Task<IActionResult> RecentUsers()
        {
            var data = await GetAsync<List<RecentUserItem>>("api/admin/dashboard/recent-users");

            return Json(data);
        }

        [HttpGet("dashboard/recent-jobs")]
        public async Task<IActionResult> RecentJobs()
        {
            var data = await GetAsync<List<RecentJobItem>>("api/admin/dashboard/recent-jobs");

            return Json(data);
        }

        [HttpGet("report-list")]
        public async Task<IActionResult> ReportList([FromQuery]ReportStatus? status, [FromQuery] int page = 1)
        {
            try
            {
                int pageSize = 10;
                var queryParams = new List<KeyValuePair<string, string>>();
                if (status.HasValue)
                {
                    queryParams.Add(new("status", ((int)status.Value).ToString()));
                }
                queryParams.Add(new("page", page.ToString()));
                queryParams.Add(new("pageSize", pageSize.ToString()));

                var url = QueryHelpers.AddQueryString("api/admin/reports", queryParams);
                var data = await GetAsync<PaginateResult<ReportDto>>(url);
                ViewData["StatusFilter"] = status;
                return View(data ?? new PaginateResult<ReportDto>());
            }
            catch (Exception e)
            {
                TempData["Error"] = "Cannot load report list: " + e.Message;
                return RedirectToAction("Dashboard");
            }
        }
        [HttpPost("reports/{id}/status")]
        public async Task<IActionResult> UpdateReportStatus(int id, int newStatus, string returnUrl)
        {
            try
            {
                var isSuccess = await PutAsync($"api/admin/reports/{id}/status", newStatus);
                if (isSuccess)
                {
                    TempData["Success"] = "Report status updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update report status.";
                }
            }
            catch (Exception e)
            {
                TempData["Error"] = "An error occurred: " + e.Message;
            }

            return Redirect(returnUrl ?? "/admin/report-list");
        }
    }
}
