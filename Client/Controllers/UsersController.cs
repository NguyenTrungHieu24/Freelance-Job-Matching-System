using BusinessObjects.Common;
using BusinessObjects.DTOs;
using Client.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;

namespace Client.Controllers
{
    [Route("users")]
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : BaseController
    {
        public UsersController(IHttpClientFactory factory) : base(factory)
        {
        }

        public async Task<IActionResult> Index(FilterUserDTO filter, [FromQuery] int? page)
        {
            try
            {
                if (page != null)
                {
                    filter.Page = (int)page;
                }

                var queries = BuildQueryParams(filter);

                var url = QueryHelpers.AddQueryString("api/users", queries);

                var data = await GetAsync<PaginateResult<UserDto>>(url);

                var roles = await GetAsync<List<RoleDTO>>("api/roles");

                Console.WriteLine(roles.Count);

                ViewBag.Roles = new SelectList(roles, "Id", "Name");

                return View(new ListUsersModel
                {
                    Filter = filter,
                    Users = data ?? new PaginateResult<UserDto>()
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Cannot load jobs: {ex.Message}";

                return View(new ListUsersModel
                {
                    Filter = filter,
                    Users = new PaginateResult<UserDto>()
                });
            }
        }

        private static List<KeyValuePair<string, string>> BuildQueryParams(FilterUserDTO filter)
        {
            var queryParams = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                queryParams.Add(new KeyValuePair<string, string>("keyword", filter.Keyword));

            if (filter.Status.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("status", ((int)filter.Status.Value).ToString()));

            if (filter.CreatedFrom.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("createdFrom", filter.CreatedFrom.Value.ToString("yyyy-MM-dd")));

            if (filter.CreatedTo.HasValue)
                queryParams.Add(new KeyValuePair<string, string>("createdTo", filter.CreatedTo.Value.ToString("yyyy-MM-dd")));

            if (filter.RoleIds.Count > 0)
            {
                foreach (var roleId in filter.RoleIds)
                {
                    queryParams.Add(new KeyValuePair<string, string>(
                       "roleIds",
                       roleId.ToString())
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

        [HttpPost]
        [Route("deactivate")]
        public async Task<IActionResult> Deactivate([FromQuery] int id)
        {
            try
            {
                var result = await PostAsync<string, ApiResult<bool>>($"api/users/deactivate/{id}", null);

                if (!result.Success)
                {
                    return StatusCode(500, result);
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<bool>.Fail(ex.Message));
            }
        }

        [HttpPost]
        [Route("activate")]
        public async Task<IActionResult> Activate([FromQuery] int id)
        {
            try
            {
                var result = await PostAsync<string, ApiResult<bool>>($"api/users/activate/{id}", null);

                if (!result.Success)
                {
                    return StatusCode(500, result);
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<bool>.Fail(ex.Message));
            }
        }
    }
}
