using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

public class FreelancerController : Controller
{
    // GET
    public IActionResult Dashboard()
    {
        return View();
    }
}