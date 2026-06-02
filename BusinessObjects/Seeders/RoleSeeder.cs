using BusinessObjects.Enums;
using BusinessObjects.Models;

namespace BusinessObjects.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.Roles.Any())
                return;

            var Roles = Enum.GetValues<RoleEnum>()
                .Select(Role => new Role
                {
                    Id = (int)Role,
                    Name = Role.ToString()
                })
                .ToList();

            context.Roles.AddRange(Roles);

            await context.SaveChangesAsync();
        }
    }
}
