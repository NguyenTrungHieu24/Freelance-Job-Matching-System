using BusinessObjects.Enums;
using BusinessObjects.Models;

namespace BusinessObjects.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var existingRoleIds = context.Roles.Select(r => r.Id).ToList();

            var newRoles = Enum.GetValues<RoleEnum>()
                .Where(r => !existingRoleIds.Contains((int)r))
                .Select(Role => new Role
                {
                    Id = (int)Role,
                    Name = Role.ToString()
                })
                .ToList();

            if (newRoles.Any())
            {
                context.Roles.AddRange(newRoles);
                await context.SaveChangesAsync();
            }
        }
    }
}
