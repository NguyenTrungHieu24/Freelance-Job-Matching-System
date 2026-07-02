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
                // Fetch 6 latest ACTIVE jobs for the homepage (no auth required)
                var result = await GetAsync<JobPaginateResult>(
                    "/api/jobs?Page=1&PageSize=6&Status=ACTIVE");
                ViewBag.LatestJobs = result?.Items ?? new List<HomeJobDto>();
            }
            catch (Exception ex)
            {
                ViewBag.LatestJobs = new List<HomeJobDto>();
                ViewBag.ErrorMessage = ex.Message + "\n" + ex.StackTrace;
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

    // Lightweight DTOs for deserializing the API response on the client side
    public class HomeJobDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Budget { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EmployerName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public List<string> Skills { get; set; } = new();
    }

    public class JobPaginateResult
    {
        public List<HomeJobDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}