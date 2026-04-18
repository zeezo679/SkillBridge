using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoftBridge.Domain.DbInitializer;
using SoftBridge.Domain.Models.User;
using SoftBridge.Persistence.Seeds;

namespace SoftBridge.Persistence.Implements.InitializerImplement
{
    public class DbInitialized(ProjectDbContext projectDbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : IDbInitializer
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
            // seed roles
            await SeederAsync.SeedRolesAsync(roleManager);
            // seed admin
            await SeederAsync.SeedAdminUserAsync(userManager);
            //  Seed Dummy Providers and Clients
            await SeederAsync.SeedDummyUsersAsync(userManager, projectDbContext);
            // seed categories
            await SeederAsync.SeedDummyCategoriesAsync(userManager, projectDbContext);
        }
    }
}