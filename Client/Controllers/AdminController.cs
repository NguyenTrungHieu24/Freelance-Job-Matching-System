using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Client.Models.Admin;
using Client.Models.Skills;
using Client.Models.Jobs;
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

        [HttpGet("jobs")]
        public async Task<IActionResult> Jobs(FilterJobDTO filter, [FromQuery] int? page)
        {
            try
            {
                if (page.HasValue)
                {
                    filter.Page = page.Value;
                }
                filter.Page = filter.Page <= 0 ? 1 : filter.Page;
                filter.PageSize = 10;

                var queryParams = new List<KeyValuePair<string, string>>
                {
                    new("page", filter.Page.ToString()),
                    new("pageSize", filter.PageSize.ToString())
                };

                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                {
                    queryParams.Add(new("keyword", filter.Keyword));
                }

                var url = QueryHelpers.AddQueryString("api/jobs", queryParams);
                var data = await GetAsync<PaginateResult<JobDTO>>(url);

                return View(new ListJobsModel
                {
                    Filter = filter,
                    Jobs = data ?? new PaginateResult<JobDTO>()
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Cannot load jobs: " + ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost("jobs/toggle-status/{id}")]
        public async Task<IActionResult> ToggleJobStatus(int id, string returnUrl)
        {
            try
            {
                var isSuccess = await PostAsync<string, ApiResult<bool>>($"api/jobs/admin/toggle-status/{id}", null);
                if (isSuccess.Success)
                {
                    TempData["Success"] = "Đã cập nhật trạng thái tin tuyển dụng!";
                }
                else
                {
                    TempData["Error"] = "Thao tác thất bại: " + isSuccess.Message;
                }
            }
            catch (Exception e)
            {
                TempData["Error"] = "Đã xảy ra lỗi: " + e.Message;
            }

            return Redirect(returnUrl ?? "/admin/jobs");
        }

        [HttpGet("payments")]
        public async Task<IActionResult> Payments([FromQuery] string? keyword, [FromQuery] int page = 1)
        {
            try
            {
                int pageSize = 10;
                var queryParams = new List<KeyValuePair<string, string>>
                {
                    new("page", page.ToString()),
                    new("pageSize", pageSize.ToString())
                };

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    queryParams.Add(new("keyword", keyword));
                }

                var url = QueryHelpers.AddQueryString("api/admin/payments", queryParams);
                var data = await GetAsync<PaginateResult<AdminPaymentDto>>(url);
                ViewData["Keyword"] = keyword;

                return View(data ?? new PaginateResult<AdminPaymentDto>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể tải danh sách lịch sử thanh toán: " + ex.Message;
                return RedirectToAction("Dashboard");
            }
        }
    }
}
