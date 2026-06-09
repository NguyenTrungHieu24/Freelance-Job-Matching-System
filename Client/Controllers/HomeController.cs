using BusinessObjects.Common;
using BusinessObjects.DTOs;
using Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;

namespace Client.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IHttpClientFactory factory) : base(factory)
        {
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch latest 6 active jobs sorted by newest first
                var queryParams = new Dictionary<string, string?>
                {
                    ["sortBy"] = "CreatedAt",
                    ["isDescending"] = "true",
                    ["page"] = "1",
                    ["pageSize"] = "6"
                };
                var url = QueryHelpers.AddQueryString("api/jobs", queryParams);
                var data = await GetAsync<PaginateResult<JobDTO>>(url);
                ViewBag.LatestJobs = data?.Items ?? new List<JobDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HomeController] Error fetching latest jobs: {ex.Message}");
                ViewBag.LatestJobs = new List<JobDTO>();
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}