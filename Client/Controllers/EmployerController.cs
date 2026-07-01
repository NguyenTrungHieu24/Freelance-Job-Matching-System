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
            var data = await GetAsync<EmployerDashboard>("api/employer/dashboard");
            return View(data);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Cannot load dashboard: " + ex.Message;
            return View(new EmployerDashboard());
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
                FullName = model.Employer.FullName,
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


    // GET: /employer/applications
    [HttpGet("applications")]
    public async Task<IActionResult> Applications()
    {
        try
        {
            // Goi API lay ds don Apply
            var list = await GetAsync<List<EmployerApplicationDto>>("api/employer/applications");
            return View(list ?? new List<EmployerApplicationDto>());
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải danh sách đơn ứng tuyển: " + ex.Message;
            return View(new List<EmployerApplicationDto>());
        }
    }

    // POST: /employer/applications/approve
    [HttpPost("applications/approve")]
    public async Task<IActionResult> Approve(int id, string status)
    {
        try
        {
            // Goi api cap nhat trang thai ung tuyen
            // status truyen len: ACCEPTED = 2 hoac REJECTED = 3
            var url = $"api/employer/applications/{id}/status?status={status}";
            var success = await PutAsync<object>(url, new { });

            if (success)
            {
                TempData["Success"] = status == "2" ? "Đã duyệt nhận ứng viên thành công!" : "Đã từ chối đơn ứng tuyển.";
            }
            else
            {
                TempData["Error"] = "Cập nhật trạng thái đơn thất bại.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
        }
        return RedirectToAction("Applications");
    }


    [HttpGet("jobs/create")]
    public async Task<IActionResult> CreateJob()
    {
        try
        {
            var vm = new CreateJobViewModel
            {
                Deadline = DateTime.Today.AddDays(7),
                Categories = await GetAsync<List<CategoryDTO>>("api/categories"),
                Skills = await GetAsync<List<SkillDTO>>("api/skills/all")
            };

            return View(vm);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(MyJobs));
        }
    }

    [HttpPost("jobs/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJob(CreateJobViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await GetAsync<List<CategoryDTO>>("api/categories");
            model.Skills = await GetAsync<List<SkillDTO>>("api/skills/all");

            return View(model);
        }

        await PostAsync<CreateJobDto, JobDTO>("api/jobs", model);

        return RedirectToAction(nameof(MyJobs));
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> MyJobs()
    {
        try
        {
            var jobs = await GetAsync<List<JobDTO>>("api/jobs/my");

            return View(jobs);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(new List<JobDTO>());
        }
    }
}