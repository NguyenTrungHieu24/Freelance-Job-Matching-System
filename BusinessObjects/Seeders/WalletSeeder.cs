using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessObjects.Seeders
{
    public static class WalletSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Ensure FINANCE_MANAGER role exists in the Roles table
            var roleExists = await context.Roles.AnyAsync(r => r.Id == (int)RoleEnum.FINANCE_MANAGER);
            if (!roleExists)
            {
                context.Roles.Add(new Role 
                { 
                    Id = (int)RoleEnum.FINANCE_MANAGER, 
                    Name = RoleEnum.FINANCE_MANAGER.ToString() 
                });
                await context.SaveChangesAsync();
            }

            // 1. Seed tai khoan Finance Manager neu chua co
            var fmExists = await context.Users
                .AnyAsync(u => u.Email == "finance@toplancer.vn");

            if (!fmExists)
            {
                var fm = new User
                {
                    FullName = "Finance Manager",
                    Email = "finance@toplancer.vn",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Finance@123"),
                    RoleId = (int)RoleEnum.FINANCE_MANAGER,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Users.Add(fm);
                await context.SaveChangesAsync();
            }

            // 2. Tao vi cho tat ca Employer + Freelancer chua co vi
            var usersWithoutWallet = await context.Users
                .Where(u => (u.RoleId == (int)RoleEnum.EMPLOYER
                          || u.RoleId == (int)RoleEnum.FREELANCER)
                         && !context.Wallets.Any(w => w.UserId == u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            if (usersWithoutWallet.Any())
            {
                var wallets = usersWithoutWallet.Select(userId => new Wallet
                {
                    UserId = userId,
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                context.Wallets.AddRange(wallets);
                await context.SaveChangesAsync();
            }
        }
    }
}
