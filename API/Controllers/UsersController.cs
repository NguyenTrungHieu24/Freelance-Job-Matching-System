using API.Services.Auth;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects;
using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : BaseController
    {
        public UsersController(AppDbContext context, IMapper mapper, IUserService user) : base(context, mapper, user)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] FilterUserDTO filter)
        {
            var query = _context.Users
                .Include(x => x.Role)
                .AsQueryable();

            #region Filters

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                query = query.Where(x =>
                    x.FullName.Contains(filter.Keyword) ||
                    x.Email.Contains(filter.Keyword));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.IsActive == (filter.Status.Value == 1));
            }

            if (filter.RoleIds.Count > 0)
            {
                query = query.Where(x => filter.RoleIds.Contains(x.RoleId));
            }

            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(x =>
                    x.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(x =>
                    x.CreatedAt <= filter.CreatedTo.Value);
            }

            #endregion

            #region Sorting

            query = filter.SortBy?.ToLower() switch
            {
                "fullname" => filter.IsDescending
                    ? query.OrderByDescending(x => x.FullName)
                    : query.OrderBy(x => x.FullName),

                "email" => filter.IsDescending
                    ? query.OrderByDescending(x => x.Email)
                    : query.OrderBy(x => x.Email),

                _ => filter.IsDescending
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt)
            };

            #endregion

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(new PaginateResult<UserDto>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(
                _mapper.Map<UserDto>(user)
            );
        }

        [HttpPost("deactivate/{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return Ok(ApiResult<bool>.Fail("User not found"));
            }

            if ((RoleEnum)user.RoleId == RoleEnum.ADMIN)
            {
                return Ok(ApiResult<bool>.Fail("Cannot deactivate admin user"));
            }

            user.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok(ApiResult<bool>.Ok(true, "Deactivate user successfully!"));
        }

        [HttpPost("activate/{id}")]
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return Ok(ApiResult<bool>.Fail("User not found"));
            }

            user.IsActive = true;

            await _context.SaveChangesAsync();

            return Ok(ApiResult<bool>.Ok(true, "Activate user successfully!"));
        }
    }
}
