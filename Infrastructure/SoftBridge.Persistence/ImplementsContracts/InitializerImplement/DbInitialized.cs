using SoftBridge.Domain.DbInitializer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoftBridge.Persistence.Seeds;

namespace SoftBridge.Persistence.Implements.InitializerImplement
{
    public class DbInitialized(ProjectDbContext projectDbContext , RoleManager<IdentityRole> roleManager) : IDbInitializer
    {
        public async Task DataSeedAsync()
        {
            try
            {
                var pendingMigrations = await projectDbContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations != null && pendingMigrations.Any())
                    await projectDbContext.Database.MigrateAsync();
            }
            catch (Exception)
            {
                // Log the exception or handle it as needed
                throw;
            }
            await SeederAsync.SeedRolesAsync(roleManager);
        }
    }
}