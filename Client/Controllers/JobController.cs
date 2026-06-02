using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Client.Controllers
{
    public class JobController : Controller
    {
        private readonly HttpClient _httpClient;

        public JobController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(
                "https://localhost:7001/api/jobs"
            );

            if (!response.IsSuccessStatusCode)
            {
                return View(new List<string>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var jobs = JsonSerializer.Deserialize<List<string>>(json);

            return View(jobs);
        }
    }
}
