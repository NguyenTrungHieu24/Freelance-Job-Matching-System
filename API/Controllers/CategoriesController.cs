using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(
            AppDbContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>>
            GetCategories()
        {
            var categories = await _context.Categories
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/categories/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>>
            GetCategory(int id)
        {
            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found"
                });
            }

            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<Category>>
            CreateCategory(
                [FromBody] Category category)
        {
            var exists = await _context.Categories
                .AnyAsync(x => x.Name == category.Name);

            if (exists)
            {
                return BadRequest(new
                {
                    message = "Category already exists"
                });
            }

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.Id },
                category
            );
        }

        // PUT: api/categories/1
        [HttpPut("{id}")]
        public async Task<ActionResult>
            UpdateCategory(
                int id,
                [FromBody] Category updatedCategory)
        {
            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found"
                });
            }

            category.Name = updatedCategory.Name;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully"
            });
        }

        // DELETE: api/categories/1
        [HttpDelete("{id}")]
        public async Task<ActionResult>
            DeleteCategory(int id)
        {
            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found"
                });
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully"
            });
        }
    }
}