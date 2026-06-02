using API.Services.Auth;
using API.Services.Memory;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/skills")]
    [ApiController]
    public class SkillsController : BaseController
    {
        protected ICacheService _cache;
        public SkillsController(AppDbContext context, IMapper mapper, IUserService user, ICacheService cache) : base(context, mapper, user)
        {
            _cache = cache;
        }


        // GET: api/skills
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<PaginateResult<SkillDTO>>> GetSkills([FromQuery] FilterSkillDTO q)
        {
            var query = _context.Skills.AsQueryable();

            query = BindingQuery(query, q);
            var totalItems = await query.CountAsync();


            var skills = await query
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .ToListAsync();

            return Ok(new PaginateResult<SkillDTO>
            {
                Items = _mapper.Map<List<SkillDTO>>(skills),
                PageNumber = q.Page,
                PageSize = q.PageSize,
                TotalItems = totalItems
            });
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<SkillDTO>>> GetAllSkills()
        {
            return Ok(await _cache.GetOrCreateAsync(
                    "skills_all",
                    async () =>
                    {
                        return _mapper.Map<List<SkillDTO>>(
                            await _context.Skills
                                .OrderBy(x => x.Name)
                                .ToListAsync());
                    },
                    TimeSpan.FromHours(1))
                );
        }


        private IQueryable<Skill> BindingQuery(IQueryable<Skill> query, FilterSkillDTO args)
        {
            if (!string.IsNullOrWhiteSpace(args.Keyword))
            {
                query = query.Where(e => e.Name.Contains(args.Keyword));
            }
            else
            {
            }

            return query;
        }

        // GET: api/skills/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Skill>> GetSkill(int id)
        {
            var skill = await _context.Skills
                .FindAsync(id);

            if (skill == null)
            {
                return NotFound(new
                {
                    message = "Skill not found"
                });
            }

            return Ok(skill);
        }

        // POST: api/skills
        [HttpPost]
        public async Task<ActionResult<Skill>> CreateSkill([FromBody] Skill skill)
        {
            _context.Skills.Add(skill);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetSkill),
                new { id = skill.Id },
                skill
            );
        }

        // PUT: api/skills/1
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateSkill(int id, [FromBody] Skill updatedSkill)
        {
            var skill = await _context.Skills
                .FindAsync(id);

            if (skill == null)
            {
                return NotFound(new
                {
                    message = "Skill not found"
                });
            }

            skill.Name = updatedSkill.Name;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Skill updated successfully"
            });
        }

        // DELETE: api/skills/1
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSkill(int id)
        {
            var skill = await _context.Skills
                .FindAsync(id);

            if (skill == null)
            {
                return NotFound(new
                {
                    message = "Skill not found"
                });
            }

            _context.Skills.Remove(skill);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Skill deleted successfully"
            });
        }
    }
}