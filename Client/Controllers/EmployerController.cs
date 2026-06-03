using BusinessObjects.DTOs;
using Client.Models.Employer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

[Authorize(Roles = "EMPLOYER")]
[Route("employer")]
public class EmployerController : BaseController
{
    public EmployerController(IHttpClientFactory factory) : base(factory)
    {
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var data = await GetAsync<DashBoardDto>("api/employer/dashboard/summary");
            var model = new EmployerDashboard
            {
                Dashboard = data
            };
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Cannot load dashboard: " + ex.Message;
            return View(new EmployerDashboard { Dashboard = new DashBoardDto() });
        }
    }

    [HttpGet("personal-info")]
    public async Task<IActionResult> PersonalInfo()
    {
        try
        {
            var data = await GetAsync<EmployerDto>("api/employer/personal-info");
            var model = new EmployerPersonalInfoViewModel
            {
                Employer = data,
            };
            return View(model);
        }
        catch (Exception e)
        {
            TempData["Error"] = "Cannot load personal info: " + e.Message;
            return RedirectToAction("PersonalInfo", "Employer");
        }
    }

    [HttpGet("personal-info/edit")]
    public async Task<IActionResult> PersonalInfoEdit()
    {
        try
        {
            var data = await GetAsync<EmployerDto>("api/employer/personal-info");
            var model = new EmployerPersonalInfoViewModel
            {
                Employer = data
            };
            return View(model);
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("PersonalInfo", "Employer");
        }
    }

    [HttpPost("personal-info/edit")]
    public async Task<IActionResult> PersonalInfoEdit(EmployerPersonalInfoViewModel model)
    {
        try
        {
            if (model.LogoImg != null && model.LogoImg.Length > 0)
            {
                var successUpload = await UploadFileAsync("api/employer/personal-info/logo-upload", model.LogoImg);
                if (!successUpload)
                {
                    TempData["Error"] = "Cannot upload logo file";
                    return RedirectToAction("PersonalInfo", "Employer");
                }
            }
            var dto = new UpdateEmployerProfileDto
            {
                FullName = model.Employer.FullName ,
                CompanyName = model.Employer.CompanyName,
                Description = model.Employer.Description,
                Email = model.Employer.Email,
                Phone = model.Employer.Phone,
                Address = model.Employer.Address,
            };
                
            var success = await PutAsync("api/employer/personal-info", dto);
            if (success)
            {
                TempData["Success"] = "Update personal settings successfully";
                return RedirectToAction("PersonalInfo", "Employer");
            }
                
            TempData["Error"] = "Error updating settings";
            return RedirectToAction("PersonalInfo", "Employer");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("PersonalInfo", "Employer");
        }
    }
}