using BusinessObjects.Common;
using BusinessObjects.DTOs;
using Client.Models.Employer;
using Client.Models.Jobs;
using Client.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;

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

    // POST: /employer/applications/complete
    [HttpPost("applications/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            var success = await PostAsync<object, object>($"api/employer/applications/{id}/complete", new { });
            TempData["Success"] = "Xác nhận hoàn thành job và thanh toán thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Thanh toán thất bại: " + ex.Message;
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

            return View("JobForm", vm);
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

            return View("JobForm", model);
        }

        try
        {
            await PostAsync<CreateJobViewModel, JobDTO>("api/jobs", model);
            TempData["Success"] = "Đăng tin tuyển dụng thành công!";
            return RedirectToAction(nameof(MyJobs));
        }
        catch (Exception ex)
        {
            string errorMessage = ex.Message;
            try
            {
                using (var doc = System.Text.Json.JsonDocument.Parse(ex.Message))
                {
                    if (doc.RootElement.TryGetProperty("message", out var msgProp))
                    {
                        errorMessage = msgProp.GetString() ?? ex.Message;
                    }
                }
            }
            catch
            {
                // Ignore JSON parsing errors
            }

            ModelState.AddModelError(string.Empty, errorMessage);
            model.Categories = await GetAsync<List<CategoryDTO>>("api/categories");
            model.Skills = await GetAsync<List<SkillDTO>>("api/skills/all");
            return View("JobForm", model);
        }
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> MyJobs(FilterJobDTO filter, [FromQuery] int? page)
    {
        try
        {
            // override page from query
            if (page.HasValue)
            {
                filter.Page = page.Value;
            }

            filter.Page = filter.Page <= 0 ? 1 : filter.Page;

            // build query string
            var queries = BuildQueryParams(filter);

            var url = QueryHelpers.AddQueryString("api/jobs", queries);

            // parallel calls (FAST UI)
            var jobsTask = GetAsync<PaginateResult<JobDTO>>(url);
            var skillsTask = GetAsync<List<SkillDTO>>("api/skills/all");

            await Task.WhenAll(jobsTask, skillsTask);

            var jobs = jobsTask.Result;
            var skills = skillsTask.Result;

            ViewBag.Skills = new SelectList(skills, "Id", "Name");

            return View(new ListJobsModel
            {
                Filter = filter,
                Jobs = jobs ?? new PaginateResult<JobDTO>()
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Cannot load jobs: {ex.Message}";

            return View(new ListJobsModel
            {
                Filter = filter,
                Jobs = new PaginateResult<JobDTO>()
            });
        }
    }

    [HttpGet("job/details/{id}")]
    public async Task<IActionResult> JobDetails(int id)
    {
        try
        {
            var job = await GetAsync<JobDTO>($"api/jobs/{id}");

            if (job == null)
            {
                return NotFound();
            }

            return View(new JobDetailViewModel
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Budget = job.Budget,
                Status = job.Status,
                Deadline = job.Deadline,
                CreatedAt = job.CreatedAt,
                EmployerProfileId = job.EmployerProfileId,
                EmployerName = job.EmployerName,
                CategoryId = job.CategoryId,
                CategoryName = job.CategoryName,
                ApplicationsCount = job.ApplicationsCount,
                Skills = job.Skills,
                Applications = job.Applications
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("MyJobs");
        }
    }


    [HttpGet("job/edit/{id}")]
    public async Task<IActionResult> EditJob(int id)
    {
        try
        {
            var job = await GetAsync<JobDTO>($"api/jobs/{id}");

            var categories = await GetAsync<List<CategoryDTO>>("api/categories");
            var skills = await GetAsync<List<SkillDTO>>("api/skills/all");

            var model = new CreateJobViewModel
            {
                Title = job.Title,
                Description = job.Description,
                Budget = job.Budget,
                Deadline = job.Deadline,
                CategoryId = job.CategoryId,

                SkillIds = job.Skills != null
                    ? skills.Where(s => job.Skills.Contains(s.Name)).Select(s => s.Id).ToList()
                    : new List<int>(),

                Categories = categories,
                Skills = skills
            };

            ViewBag.IsEdit = true;
            ViewBag.JobId = id;

            return View("JobForm", model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("MyJobs");
        }
    }

    [HttpPost("job/edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJob(int id, CreateJobViewModel model)
    {
        try
        {
            var dto = new UpdateJobDto
            {
                Title = model.Title,
                Description = model.Description,
                Budget = model.Budget,
                CategoryId = model.CategoryId,
                Deadline = model.Deadline,
                Skills = model.SkillIds
            };

            await PutAsync($"api/jobs/{id}", dto);

            TempData["Success"] = "Job updated successfully";

            return RedirectToAction("JobDetails", new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;

            // reload dropdowns
            model.Categories = await GetAsync<List<CategoryDTO>>("api/categories");
            model.Skills = await GetAsync<List<SkillDTO>>("api/skills/all");

            ViewBag.IsEdit = true;
            ViewBag.JobId = id;

            return View("JobForm", model);
        }
    }

    private static List<KeyValuePair<string, string>> BuildQueryParams(FilterJobDTO filter)
    {
        var queryParams = new List<KeyValuePair<string, string>>();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            queryParams.Add(new KeyValuePair<string, string>("keyword", filter.Keyword.Trim()));

        if (!string.IsNullOrWhiteSpace(filter.EmployerKeyword))
            queryParams.Add(new KeyValuePair<string, string>("employerKeyword", filter.EmployerKeyword.Trim()));

        if (filter.Status.HasValue)
            queryParams.Add(new KeyValuePair<string, string>("status", ((int)filter.Status.Value).ToString()));

        if (filter.CategoryId.HasValue)
            queryParams.Add(new KeyValuePair<string, string>("categoryId", filter.CategoryId.ToString()));

        if (filter.EmployerProfileId.HasValue)
            queryParams.Add(new KeyValuePair<string, string>("employerProfileId", filter.EmployerProfileId.ToString()));

        if (filter.MinBudget.HasValue)
            queryParams.Add(new KeyValuePair<string, string>("minBudget", filter.MinBudget.ToString()));

        if (filter.MaxBudget.HasValue)
            queryParams.Add(new KeyValuePair<string, string>("maxBudget", filter.MaxBudget.ToString()));

        if (filter.CreatedFrom.HasValue)
            queryParams.Add(new KeyValuePair<string, string>("createdFrom", filter.CreatedFrom.Value.ToString("yyyy-MM-dd")));

        if (filter.Temperature.HasValue)
            queryParams.Add(new KeyValuePair<string, string>("temperature", filter.Temperature.Value.ToString()));

        if (filter.CreatedTo.HasValue)
            queryParams.Add(new KeyValuePair<string, string>("createdTo", filter.CreatedTo.Value.ToString("yyyy-MM-dd")));

        if (filter.DeadlineFrom.HasValue)
            queryParams.Add(new KeyValuePair<string, string>("deadlineFrom", filter.DeadlineFrom.Value.ToString("yyyy-MM-dd")));

        if (filter.DeadlineTo.HasValue)
            queryParams.Add(new KeyValuePair<string, string>("deadlineTo", filter.DeadlineTo.Value.ToString("yyyy-MM-dd")));

        if (filter.SkillIds.Count > 0)
        {
            foreach (var skillId in filter.SkillIds)
            {
                queryParams.Add(new KeyValuePair<string, string>(
                   "skillIds",
                   skillId.ToString())
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.SortBy))
            queryParams.Add(new KeyValuePair<string, string>("sortBy", filter.SortBy));

        queryParams.Add(new KeyValuePair<string, string>("isDescending", filter.IsDescending.ToString()));
        queryParams.Add(new KeyValuePair<string, string>("page", (filter.Page == 0 ? 1 : filter.Page).ToString()));
        queryParams.Add(new KeyValuePair<string, string>("pageSize", filter.PageSize.ToString()));

        return queryParams;
    }

    [HttpGet("freelancer-profile/{id}")]
    public async Task<IActionResult> FreelancerProfile(int id)
    {
        try
        {
            var profile = await GetAsync<FreelancerCvDto>($"api/employer/freelancer-profile/{id}");
            return View(profile);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải hồ sơ ứng viên: " + ex.Message;
            return RedirectToAction("Applications");
        }
    }

    [HttpPost("reviews/create")]
    public async Task<IActionResult> CreateReview(CreateReviewDto dto)
    {
        try
        {
            var success = await PostAsync<CreateReviewDto, object>("api/reviews", dto);
            TempData["Success"] = "Đánh giá ứng viên thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Đăng đánh giá thất bại: " + ex.Message;
        }
        return RedirectToAction("Applications");
    }

    [HttpGet("change-password")]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            var success = await PutAsync("api/auth/change-password", new
            {
                OldPassword = model.OldPassword,
                NewPassword = model.NewPassword,
                ConfirmPassword = model.ConfirmPassword
            });

            if (success)
            {
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction("ChangePassword");
            }

            TempData["Error"] = "Failed to change password";
            return RedirectToAction("ChangePassword");
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("ChangePassword");
        }
    }
}