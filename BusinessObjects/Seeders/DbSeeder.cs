using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Seeders
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            AppDbContext context
        )
        {
            try
            {
                await context.Database.EnsureCreatedAsync();

                await RoleSeeder.SeedAsync(context);

                await UserSeeder.SeedAsync(context);

                await EmployerProfileSeeder.SeedAsync(context);

                await FreelancerProfileSeeder.SeedAsync(context);

                await SkillSeeder.SeedAsync(context);

                await CategorySeeder.SeedAsync(context);

                await JobSeeder.SeedAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
