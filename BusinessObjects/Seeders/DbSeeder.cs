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
            await context.Database.EnsureCreatedAsync();

            await RoleSeeder.SeedAsync(context);

            await UserSeeder.SeedAsync(context);

            await SkillSeeder.SeedAsync(context);

            await CategorySeeder.SeedAsync(context);
        }
    }
}
