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

            var roles = new List<Role>{
                new Role {
                    Id = (int)RoleEnum.ADMIN,
                    Name = RoleEnum.ADMIN.ToString(),
                },
                new Role {
                    Id = (int)RoleEnum.EMPLOYER,
                    Name = RoleEnum.EMPLOYER.ToString(),
                },
                new Role {
                    Id = (int)RoleEnum.FREELANCER,
                    Name = RoleEnum.FREELANCER.ToString(),
                },
                new Role {
                    Id = (int)RoleEnum.GUEST,
                    Name = RoleEnum.GUEST.ToString(),
                }
            };

            context.Roles.AddRange(roles);

            await context.SaveChangesAsync();
        }
    }
}
