using API.Services.Auth;
using API.Services.Memory;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Common;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/roles")]
    [ApiController]
    public class RolesController : BaseController
    {
        protected ICacheService _cache;
        public RolesController(AppDbContext context, IMapper mapper, IUserService user, ICacheService cache) : base(context, mapper, user)
        {
            _cache = cache;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<RoleDTO>>> GetAll()
        {
            return Ok(await _cache.GetOrCreateAsync(
                    "roles_all",
                    async () =>
                    {
                        return _mapper.Map<List<RoleDTO>>(
                            await _context.Roles
                                .OrderBy(x => x.Name)
                                .ToListAsync());
                    },
                    TimeSpan.FromHours(1))
                );
        }
    }
}
