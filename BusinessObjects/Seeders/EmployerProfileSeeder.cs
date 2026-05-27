using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects.Seeders;

public class EmployerProfileSeeder
{
    private static readonly string[] Companies =
    [
        "TechVision Solutions",
        "NextGen Software",
        "BlueOcean Digital",
        "Pixel Studio",
        "Cloudify Systems",
        "SmartHub Agency",
        "NovaTech",
        "Creative Labs",
        "Global Soft",
        "FutureX Company"
    ];

    public static async Task SeedAsync(AppDbContext context)
    {
        var employerRoleId = (int)RoleEnum.EMPLOYER;

        var existingAccountIds = await context.EmployerProfiles
            .Select(x => x.AccountId)
            .ToListAsync();

        var users = await context.Users
            .Where(x =>
                x.RoleId == employerRoleId &&
                !existingAccountIds.Contains(x.Id)
            )
            .ToListAsync();

        if (!users.Any())
            return;

        var random = new Random();

        var profiles = users.Select(user => new EmployerProfile
        {
            AccountId = user.Id,
            CompanyName = $"{Companies[random.Next(Companies.Length)]} {random.Next(1, 999)}",
            Description = "Leading company in software and digital solutions.",
            Email = user.Email,
            Phone = $"09{random.Next(10000000, 99999999)}",
            Address = "Ho Chi Minh City, Viet Nam",
            Logo = $"https://i.pravatar.cc/300?company={user.Email}"
        }).ToList();

        await context.EmployerProfiles.AddRangeAsync(profiles);

        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded {profiles.Count} employer profiles");
    }
}