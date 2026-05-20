using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Seeders
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(
           AppDbContext context)
        {
            if (context.Skills.Any())
                return;

            var categories = new List<Category>
        {
            new() { Name = "Web Development" },
            new() { Name = "ReactJS" },
            new() { Name = "Mobile Development" },
            new() { Name = "UI/UX Design" },
            new() { Name = "DevOps" },
            new() { Name = "AI/ML" },
            new() { Name = "Data Science" }
        };

            context.Categories.AddRange(categories);

            await context.SaveChangesAsync();
        }
    }
}
