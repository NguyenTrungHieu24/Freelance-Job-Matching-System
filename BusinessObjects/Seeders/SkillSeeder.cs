using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Seeders
{
    public static class SkillSeeder
    {
        public static async Task SeedAsync(
            AppDbContext context)
        {
            if (context.Skills.Any())
                return;

            var skills = new List<Skill>
        {
            new() { Name = "ASP.NET" },
            new() { Name = "ReactJS" },
            new() { Name = "SQL Server" },
            new() { Name = "Java" },
            new() { Name = "Python" }
        };

            context.Skills.AddRange(skills);

            await context.SaveChangesAsync();
        }
    }
}
