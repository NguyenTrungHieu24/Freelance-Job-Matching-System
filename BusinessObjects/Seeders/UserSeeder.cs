using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var seedUsers = new List<SeedUser>
            {
                new()
                {
                    FullName = "System Admin",
                    Email = "system-admin@gmail.com",
                    Role = RoleEnum.ADMIN,
                    Password = "Admin@123"
                },

                new()
                {
                    FullName = "System Freelancer",
                    Email = "system-freelancer@gmail.com",
                    Role = RoleEnum.FREELANCER,
                    Password = "Freelancer@123"
                },

                new()
                {
                    FullName = "System Employer",
                    Email = "system-employer@gmail.com",
                    Role = RoleEnum.EMPLOYER,
                    Password = "Employer@123"
                }
            };

            var emails = seedUsers
                .Select(x => x.Email.ToLower())
                .ToList();

            var existingEmails = await context.Users
                .Where(x => emails.Contains(x.Email))
                .Select(x => x.Email)
                .ToListAsync();

            var usersToInsert = seedUsers
                .Where(x => !existingEmails.Contains(x.Email))
                .Select(x => new User
                {
                    FullName = x.FullName,
                    Email = x.Email.ToLower(),
                    RoleId = (int)x.Role,
                    IsActive = true,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(x.Password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                })
                .ToList();

            if (usersToInsert.Any())
            {
                await context.Users.AddRangeAsync(usersToInsert);

                await context.SaveChangesAsync();
            }
        }
    }

    public class SeedUser
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public RoleEnum Role { get; set; }

        public string Password { get; set; } = string.Empty;
    }
}