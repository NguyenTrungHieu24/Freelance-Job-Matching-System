using BusinessObjects.Common;
using BusinessObjects.DTOs;
using Client.Models.Auth;
using Client.Models.Freelancer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

[Authorize(Roles = "FREELANCER")]
    [Route("freelancer")]
    public class FreelancerController : BaseController
    {
        public FreelancerController(IHttpClientFactory factory) : base(factory) { }
        [HttpGet("dashboard")]
        public IActionResult Dashboard() => View();

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
                return RedirectToAction("Dashboard");
            }
        }
        
        
        [HttpGet("personal-info/edit")]
        public async Task<IActionResult> EditPersonalInfo()
        {
            try
            {
                var data = await GetAsync<FreelancerPersonalInfoDto>("api/freelancer/personal-info");
                var model = new FreelancerPersonalInfoViewModel { PersonalInfo = data };
                return View(model);
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
                    var successUpload = await UploadFileAsync("api/freelancer/personal-info/avatar-upload", model.ProfilePhoto);
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
    }