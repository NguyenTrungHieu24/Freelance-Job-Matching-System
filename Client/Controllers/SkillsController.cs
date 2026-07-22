using BusinessObjects.Common;
using BusinessObjects.DTOs;
using Client.Models.Skills;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Client.Controllers
{
    [Route("skills")]
    public class SkillsController : BaseController
    {
        public SkillsController(IHttpClientFactory factory) : base(factory)
        {
        }

        public async Task<IActionResult> Index(FilterSkillDTO filter)
        {
            try
            {
                var queryParams = new Dictionary<string, string?>();

                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    queryParams["keyword"] = filter.Keyword;
                
                queryParams["page"] = filter.Page.ToString();
                queryParams["pageSize"] = "10";

                var url = QueryHelpers.AddQueryString("api/skills", queryParams!);
                
                var data = await GetAsync<PaginateResult<SkillDTO>>(url);

                return View(new ListSkillsModel
                {
                    Filter = filter,
                    Skills = data ?? new PaginateResult<SkillDTO>(),
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Cannot load skills: "+ex.Message;

                return View(new ListSkillsModel
                {
                    Skills = new PaginateResult<SkillDTO>()
                });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Tên kỹ năng không được để trống!";
                return RedirectToAction("Index");
            }

            try
            {
                var success = await PostAsync<object, object>("api/skills", new { Name = name });
                if (success != null)
                {
                    TempData["Success"] = "Thêm kỹ năng thành công!";
                }
                else
                {
                    TempData["Error"] = "Thêm kỹ năng thất bại.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi: " + ParseErrorMessage(ex.Message);
            }

            return RedirectToAction("Index");
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Tên kỹ năng không được để trống!";
                return RedirectToAction("Index");
            }

            try
            {
                await PutOrThrowAsync($"api/skills/{id}", new { Name = name });
                TempData["Success"] = "Cập nhật kỹ năng thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi: " + ParseErrorMessage(ex.Message);
            }

            return RedirectToAction("Index");
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await DeleteAsync($"api/skills/{id}");
                if (success)
                {
                    TempData["Success"] = "Xóa kỹ năng thành công!";
                }
                else
                {
                    TempData["Error"] = "Xóa kỹ năng thất bại.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi: " + ParseErrorMessage(ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
