using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects.Seeders;

public class FreelancerProfileSeeder
{
    private static readonly string[] Titles =
    [
        "Frontend Developer",
        "Backend Developer",
        "Fullstack Developer",
        "UI/UX Designer",
        "Mobile App Developer",
        "DevOps Engineer",
        "QA Engineer",
        "Graphic Designer",
        "Content Writer",
        "WordPress Developer"
    ];

    private static readonly string[] Bios =
    [
        "Passionate freelancer with 3+ years of experience.",
        "Experienced developer focused on scalable systems.",
        "Creative designer specializing in modern UI.",
        "Dedicated engineer delivering high quality work.",
        "Freelancer with strong communication and teamwork skills."
    ];

    public static async Task SeedAsync(AppDbContext context)
    {
        var freelancerRoleId = (int)RoleEnum.FREELANCER;

        var existingAccountIds = await context.FreelancerProfiles
            .Select(x => x.AccountId)
            .ToListAsync();

        var users = await context.Users
            .Where(x =>
                x.RoleId == freelancerRoleId &&
                !existingAccountIds.Contains(x.Id)
            )
            .ToListAsync();

        if (!users.Any())
            return;

        var random = new Random();

        var profiles = users.Select(user => new FreelancerProfile
        {
            AccountId = user.Id,
            Title = Titles[random.Next(Titles.Length)],
            Bio = Bios[random.Next(Bios.Length)],
            Address = "Ha Noi, Viet Nam",
            Phone = $"09{random.Next(10000000, 99999999)}",
            ProfilePhoto = $"https://i.pravatar.cc/300?u={user.Email}",
            CVUrl = "https://example.com/cv.pdf",
            PortfolioUrl = "https://portfolio.example.com",
            PortfolioDescription = "Personal portfolio showcasing projects."
        }).ToList();

        await context.FreelancerProfiles.AddRangeAsync(profiles);

        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded {profiles.Count} freelancer profiles");
    }
}