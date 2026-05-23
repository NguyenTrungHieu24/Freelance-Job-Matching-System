using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Models;
using Client.Models;
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
    }
}
