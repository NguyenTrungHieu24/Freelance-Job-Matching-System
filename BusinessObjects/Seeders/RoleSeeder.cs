using BusinessObjects.Enums;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.Roles.Any())
                return;

            var roles = Enum.GetValues<RoleEnum>()
                .Select(role => new Role
                {
                    Id = (int)role,
                    Name = role.ToString()
                })
                .ToList();

            context.Roles.AddRange(roles);

            await context.SaveChangesAsync();
        }
    }
}
