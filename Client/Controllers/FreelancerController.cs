using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Client.Models.Auth;
using Client.Models.Freelancer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Client.Controllers;

[Authorize(Roles = "FREELANCER")]
[Route("freelancer")]
public class FreelancerController : BaseController
{
    public FreelancerController(IHttpClientFactory factory) : base(factory)
    {
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var data = await GetAsync<FreelancerDashboard>("api/freelancer/dashboard");
            return View(data);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Cannot load dashboard: " + ex.Message;
            return View(new FreelancerDashboard());
        }
    }

    [HttpGet("personal-info")]
    public async Task<IActionResult> PersonalInfo()
    {
        try
        {
            var data = await GetAsync<FreelancerPersonalInfoDto>("api/freelancer/personal-info");
            var model = new FreelancerPersonalInfoViewModel { PersonalInfo = data };
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Cannot load personal info: " + ex.Message;
            return RedirectToAction("Dashboard", "Freelancer");
        }
    }


    [HttpGet("personal-info/edit")]
    public async Task<IActionResult> EditPersonalInfo()
    {
        try
        {
            var data = await GetAsync<FreelancerPersonalInfoDto>("api/freelancer/personal-info");
            var model = new FreelancerPersonalInfoViewModel { PersonalInfo = data };
            return RedirectToAction("PersonalInfo", "Freelancer", model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("PersonalInfo");
        }
    }

    [HttpPost("personal-info/edit")]
    public async Task<IActionResult> EditPersonalInfo(FreelancerPersonalInfoViewModel model)
    {
        try
        {
            if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
            {
                var successUpload =
                    await UploadFileAsync("api/freelancer/personal-info/avatar-upload", model.ProfilePhoto);
                if (!successUpload)
                {
                    TempData["Error"] = "Error uploading avatar";
                    return RedirectToAction("PersonalInfo");
                }
            }

            var updateDto = new UpdateFreelancerPersonalInfoDto
            {
                FullName = model.PersonalInfo.FullName,
                Email = model.PersonalInfo.Email,
                Phone = model.PersonalInfo.Phone,
                Address = model.PersonalInfo.Address
            };
            var success = await PutAsync("api/freelancer/personal-info", updateDto);
            if (success)
            {
                TempData["Success"] = "Update personal settings successfully";
                return RedirectToAction("PersonalInfo");
            }

            TempData["Error"] = "Error updating settings";
            return RedirectToAction("PersonalInfo");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("PersonalInfo");
        }
    }

    [HttpGet("cv-portfolio")]
    public async Task<IActionResult> WorkProfile()
    {
        try
        {
            var data = await GetAsync<FreelancerCvDto>("api/freelancer/cv-portfolio");

            var skillsData = await GetAsync<PaginateResult<SkillDTO>>("api/skills?page=1&pageSize=100");
            var allSkills = skillsData?.Items ?? new List<SkillDTO>();

            var model = new FreelancerCvViewModel
            {
                CvPortfolio = data,
                AllSkills = allSkills,
                SelectedSkill = data.Skills?.Select(s => s.Id).ToList() ?? new List<int>()
            };
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Cannot load CV & Portfolio: " + ex.Message;
            return RedirectToAction("Dashboard");
        }
    }


    [HttpGet("cv-portfolio/edit")]
    public async Task<IActionResult> EditCvPortfolio()
    {
        try
        {
            var data = await GetAsync<FreelancerCvDto>("api/freelancer/cv-portfolio");
            var skillsData = await GetAsync<PaginateResult<SkillDTO>>("api/skills?page=1&pageSize=100");
            var allSkills = skillsData?.Items ?? new List<SkillDTO>();
            var model = new FreelancerCvViewModel
            {
                CvPortfolio = data,
                AllSkills = allSkills,
                SelectedSkill = data.Skills?.Select(s => s.Id).ToList() ?? new List<int>()
            };
            return RedirectToAction("WorkProfile");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("WorkProfile");
        }
    }

    [HttpPost("cv-portfolio/edit")]
    public async Task<IActionResult> EditCvPortfolio(FreelancerCvViewModel model)
    {
        try
        {
            if (model.CvFile != null && model.CvFile.Length > 0)
            {
                if (model.CvFile.Length > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "CV file size is too large (max 5MB)";
                    return RedirectToAction("WorkProfile");
                }

                var extension = Path.GetExtension(model.CvFile.FileName).ToLower();
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "PDF, doc, docx files are supported";
                    return RedirectToAction("WorkProfile");
                }

                var successUpload = await UploadFileAsync("api/freelancer/cv-portfolio/cv-upload", model.CvFile);
                if (!successUpload)
                {
                    TempData["Error"] = "Error uploading CV";
                    return RedirectToAction("WorkProfile");
                }
            }

            var updateDto = new UpdateFreelancerCvDto
            {
                Title = model.CvPortfolio?.Title,
                Bio = model.CvPortfolio?.Bio,
                PortfolioUrl = model.CvPortfolio?.PortfolioUrl,
                PortfolioDescription = model.CvPortfolio?.PortfolioDescription,
                Skills = model.SelectedSkill ?? new List<int>()
            };
            var success = await PutAsync("api/freelancer/cv-portfolio", updateDto);
            if (success)
            {
                TempData["Success"] = "Update CV & Portfolio successfully";
                return RedirectToAction("WorkProfile");
            }

            TempData["Error"] = "Error updating profile";
            return RedirectToAction("WorkProfile");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("WorkProfile");
        }
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

    private static List<KeyValuePair<string, string>> BuildQueryParams(FreelancerFilterJobDTO filter)
    {
        var q = new List<KeyValuePair<string, string>>();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            q.Add(new("Keyword", filter.Keyword));           // ✅ bỏ "Filter."

        if (filter.CategoryId.HasValue)
            q.Add(new("CategoryId", filter.CategoryId.Value.ToString()));

        if (filter.MinBudget.HasValue)
            q.Add(new("MinBudget", filter.MinBudget.Value.ToString()));

        if (filter.MaxBudget.HasValue)
            q.Add(new("MaxBudget", filter.MaxBudget.Value.ToString()));

        if (filter.Temperature.HasValue)
            q.Add(new("Temperature", ((int)filter.Temperature.Value).ToString())); // ✅ gửi số, không gửi string

        if (filter.CreatedFrom.HasValue)
            q.Add(new("CreatedFrom", filter.CreatedFrom.Value.ToString("yyyy-MM-dd")));

        if (filter.CreatedTo.HasValue)
            q.Add(new("CreatedTo", filter.CreatedTo.Value.ToString("yyyy-MM-dd")));

        if (filter.DeadlineFrom.HasValue)
            q.Add(new("DeadlineFrom", filter.DeadlineFrom.Value.ToString("yyyy-MM-dd")));

        if (filter.DeadlineTo.HasValue)
            q.Add(new("DeadlineTo", filter.DeadlineTo.Value.ToString("yyyy-MM-dd")));

        foreach (var skillId in filter.SkillIds)
            q.Add(new("SkillIds", skillId.ToString()));      // ✅ bỏ "Filter."

        if (!string.IsNullOrWhiteSpace(filter.SortBy))
            q.Add(new("SortBy", filter.SortBy));

        q.Add(new("IsDescending", filter.IsDescending.ToString().ToLower()));
        q.Add(new("Page", (filter.Page < 1 ? 1 : filter.Page).ToString()));
        q.Add(new("PageSize", filter.PageSize.ToString()));

        return q;
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> JobList(FreelancerFilterJobDTO filter, [FromQuery] int? page)
    {
        try
        {
            if (page != null) filter.Page = (int)page;

            var query = BuildQueryParams(filter);
            var url = QueryHelpers.AddQueryString("api/freelancer/jobs", query);

            var data = await GetAsync<PaginateResult<FreelancerJobDTO>>(url);
            var skills = await GetAsync<List<SkillDTO>>("api/skills/all");
            var categories = await GetAsync<List<CategoryDTO>>("api/categories");

            ViewBag.Category = categories;
            ViewBag.Skills = skills;
            return View(new FreelancerListJobsModel
            {
                Filter = filter,
                Jobs = data ?? new PaginateResult<FreelancerJobDTO>(),
            });
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return View(new FreelancerListJobsModel
            {
                Filter = filter,
                Jobs = new PaginateResult<FreelancerJobDTO>(),
            });
        }
    }

    [HttpGet("jobs/{id}")]
    public async Task<IActionResult> JobDetail(int id)
    {
        try
        {
            var job = await GetAsync<FreelancerJobDTO>($"api/freelancer/jobs/{id}");
            if (job == null) return NotFound();

            var model = new FreelancerJobDetailViewModel
            {
                Job = job,
                ApplicationForm = new CreateApplicationDto { JobId = id }
            };
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("JobList");
        }
    }

    [HttpPost("jobs/apply")]
    public async Task<IActionResult> ApplyJob(FreelancerJobDetailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Error"] = string.Join(" ", errors);
            return RedirectToAction("JobDetail", new { id = model.ApplicationForm.JobId });
        }

        try
        {
            //Xac thuc phia Client-side de upload CV
            if (model.CvFile != null && model.CvFile.Length > 0)
            {
                if (model.CvFile.Length > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "CV file size is too large (max 5MB).";
                    return RedirectToAction("JobDetail", new { id = model.ApplicationForm.JobId });
                }

                var extension = Path.GetExtension(model.CvFile.FileName).ToLower();
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Only PDF, DOC, DOCX files are supported.";
                    return RedirectToAction("JobDetail", new { id = model.ApplicationForm.JobId });
                }
            }

            var formFields = new Dictionary<string, string>
            {
                { "JobId", model.ApplicationForm.JobId.ToString() },
                { "CoverLetter", model.ApplicationForm.CoverLetter ?? "" }
            };

            var success = await PostMultipartFormAsync<bool>(
                "api/freelancer/jobs/apply",
                formFields,
                model.CvFile
            );

            if (success)
            {
                TempData["Success"] = "Applied successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to apply for the job.";
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("CoverLetter"))
            {
                TempData["Error"] = "Cover letter is required and cannot exceed the limit.";
            }
            else
            {
                TempData["Error"] = "An error occurred while applying. Please try again later.";
            }
        }

        return RedirectToAction("JobDetail", new { id = model.ApplicationForm.JobId });
    }

    [HttpGet("applications")]
    public async Task<IActionResult> Applications([FromQuery] ApplicationStatus? status, [FromQuery] int page = 1)
    {
        try
        {
            int pageSize = 5;
            var queryParams = new List<KeyValuePair<string, string>>();
            if (status.HasValue)
            {
                queryParams.Add(new("status", ((int)status.Value).ToString()));
            }
            queryParams.Add(new("page", page.ToString()));
            queryParams.Add(new("pageSize", pageSize.ToString()));

            var url = QueryHelpers.AddQueryString("api/freelancer/applications", queryParams);
            var data = await GetAsync<PaginateResult<ApplicationHistoryDto>>(url);
            ViewData["StatusFilter"] = status;
            return View(data ?? new PaginateResult<ApplicationHistoryDto>());
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Cannot load application history: " + ex.Message;
            return RedirectToAction("Dashboard");
        }
    }

    [HttpPost("applications/{id}/cancel")]
    public async Task<IActionResult> CancelApplication(int id)
    {
        try
        {
            var success = await PutAsync($"api/freelancer/application/{id}/cancel", new { });
            if (success)
            {
                TempData["Success"] = "Application cancelled successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to cancel application.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred: " + ex.Message;
        }
        return RedirectToAction("Applications");
    }

    [HttpGet("my-jobs")]
    public async Task<IActionResult> MyJobs()
    {
        try
        {
            var data = await GetAsync<List<MyJobDto>>("api/freelancer/my-jobs");
            return View(data ?? new List<MyJobDto>());
        }
        catch (Exception e)
        {
            TempData["Error"] = "Cannot load your jobs: "+e.Message;
            return View(new List<MyJobDto>());
        }
    }

    [HttpPost("reviews/create")]
    public async Task<IActionResult> CreateReview(CreateReviewDto dto)
    {
        try
        {
            var success = await PostAsync<CreateReviewDto, object>("api/reviews", dto);
            TempData["Success"] = "Đánh giá nhà tuyển dụng thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Đăng đánh giá thất bại: " + ex.Message;
        }
        return RedirectToAction("MyJobs");
    }
}