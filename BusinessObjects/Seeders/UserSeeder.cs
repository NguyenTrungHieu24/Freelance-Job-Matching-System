using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects.Seeders
{
    public class UserSeeder : JsonSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var usersToInsert = await GetInsertableUsers(context, new List<SeedUser>
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
            });

            if (usersToInsert.Any())
            {
                await context.Users.AddRangeAsync(usersToInsert);

                await context.SaveChangesAsync();
            }

            var json_users = await SeedingFromJson<SeedUserJson>("users");

            if (json_users == null || json_users.Count == 0)
            {
                return;
            }

            var jsonUsersToInsert = await GetInsertableUsers(
                context,
                json_users.Select(u => new SeedUser
                {
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role == "Freelancer" ? RoleEnum.FREELANCER : RoleEnum.EMPLOYER,
                    Password = "Admin@123",
                    CreatedAt = DateTime.TryParse(u.CreatedAt, out var dt) ? dt : DateTime.UtcNow,
                }).ToList()
            );

            if (jsonUsersToInsert.Any())
            {
                await context.Users.AddRangeAsync(jsonUsersToInsert);

                await context.SaveChangesAsync();
            }
        }

        private static async Task<List<User>> GetInsertableUsers(AppDbContext context, List<SeedUser> seedUsers)
        {
            var Emails = seedUsers
                .Select(x => x.Email.ToLower())
                .ToList();

            var existingEmails = await context.Users
                .Where(x => Emails.Contains(x.Email))
                .Select(x => x.Email)
                .ToListAsync();

            return seedUsers
                .Where(x => !existingEmails.Contains(x.Email))
                .Select(x => new User
                {
                    FullName = x.FullName,
                    Email = x.Email.ToLower(),
                    RoleId = (int)x.Role,
                    IsActive = true,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(x.Password),
                    CreatedAt = x.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                })
                .ToList();
        }
    }

    public class SeedUser
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public RoleEnum Role { get; set; }

        public string Password { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }
    }

    public class SeedUserJson
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Role { get; set; }

        public string CreatedAt { get; set; } = String.Empty;

    }
}