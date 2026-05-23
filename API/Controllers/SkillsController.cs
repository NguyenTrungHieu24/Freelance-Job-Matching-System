using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
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
        public SkillsController(AppDbContext context, IMapper mapper, IUserService user) : base(context, mapper, user)
        {
        }


        // GET: api/skills
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Skill>>>
            GetSkills()
        {
            var skills = await _context.Skills
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Ok(skills);
        }

        // GET: api/skills/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Skill>>
            GetSkill(int id)
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
        public async Task<ActionResult<Skill>>
            CreateSkill([FromBody] Skill skill)
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
        public async Task<ActionResult>
            UpdateSkill(
                int id,
                [FromBody] Skill updatedSkill)
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
        public async Task<ActionResult>
            DeleteSkill(int id)
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