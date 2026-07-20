using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.Controllers
{
    public class JobController : BaseController
    {
        public JobController(IHttpClientFactory factory) : base(factory)
        {
        }

        [HttpGet("jobs/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var job = await GetAsync<JobDto>($"api/jobs/{id}");
                if (job == null) return NotFound();

                return View("Detail", job);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể tải chi tiết công việc: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
