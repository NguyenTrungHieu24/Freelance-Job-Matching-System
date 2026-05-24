using BusinessObjects.Common;
using BusinessObjects.DTOs;
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
    }
}
