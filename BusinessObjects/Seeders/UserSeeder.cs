using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(
            AppDbContext context
        )
        {
            var adminEmail = "admin-freelancer@gmail.com";

            var existingAdmin = await context.Users
                .FirstOrDefaultAsync(
                    x => x.Email == adminEmail
                );

            if (existingAdmin != null)
                return;

            var admin = new User
            {
                FullName = "System Admin",
                Email = adminEmail,
                IsActive = true,
                RoleId = (int)RoleEnum.ADMIN,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            };

            context.Users.Add(admin);

            await context.SaveChangesAsync();
        }
    }
}
