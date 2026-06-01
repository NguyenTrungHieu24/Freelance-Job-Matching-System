using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects.Seeders
{
    public class JobSeeder : JsonSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.Jobs.Any())
                return;

            var json_jobs = await SeedingFromJson<SeedJob>("jobs");

            if (json_jobs != null && json_jobs.Any())
            {
                var employerProfileIds = await context.EmployerProfiles
                    .Select(x => x.Id)
                    .ToListAsync();

                var categoryIds = await context.Categories
                    .Select(x => x.Id)
                    .ToListAsync();

                if (!employerProfileIds.Any() || !categoryIds.Any())
                {
                    return;
                }

                await context.Jobs.AddRangeAsync(json_jobs.Select(j => new Job
                {
                    Title = j.Title,
                    Description = j.Description,
                    Budget = j.Budget,
                    Status = j.Status,
                    CreatedAt = DateTime.TryParse(j.CreatedAt, out var dt) ? dt : DateTime.UtcNow,
                    CategoryId = categoryIds[Math.Abs(j.CategoryId) % categoryIds.Count],
                    EmployerProfileId = employerProfileIds[Math.Abs(j.EmployerProfileId) % employerProfileIds.Count],
                }).ToList());

                await context.SaveChangesAsync();
            }

        }
    }

    public class SeedJob
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Budget { get; set; }
        public JobStatus Status { get; set; }

        public string CreatedAt { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public int EmployerProfileId { get; set; }
    }

}