using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Domain.Models.User;
namespace SoftBridge.Persistence.Seeds
{
    public static class SeederAsync
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in Enum.GetNames(typeof(UserType)))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            string adminEmail = "admin@softbridge.com";

            // ensure existence of admin
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail.ToUpper(),
                    Email = adminEmail,
                    FullName = "System Admin",
                    UserType = UserType.Admin,
                    EmailConfirmed = true
                };

                // create admin
                var result = await userManager.CreateAsync(newAdmin, "Admin@123456");

                // if created 
                if (result.Succeeded)
                {

                    await userManager.AddToRoleAsync(newAdmin, UserType.Admin.ToString());
                }
            }
        }

        public static async Task SeedDummyUsersAsync(UserManager<ApplicationUser> userManager, ProjectDbContext context)
        {
            // ==========================================
            // 1. 2 (Providers)

            // Provider 1:
            if (await userManager.FindByEmailAsync("provider1@softbridge.com") == null)
            {
                var p1User = new ApplicationUser
                {
                    UserName = "provider1@softbridge.com",
                    Email = "provider1@softbridge.com",
                    FullName = "Provider 1",
                    UserType = UserType.Provider,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(p1User, "Password123!");
                await userManager.AddToRoleAsync(p1User, UserType.Provider.ToString());

                context.Set<SProvider>().Add(new SProvider
                {
                    Id = Guid.NewGuid(),
                    UserId = p1User.Id,
                    Bio = "Backend Developer with 4 years of experience.",
                    Status = ProviderAccountStatus.Approved, // نخليه Approved عشان يظهر للعملا
                    TotalReviews = 0,
                    AverageRating = 0
                });
            }

            // Provider 2:
            if (await userManager.FindByEmailAsync("provider2@softbridge.com") == null)
            {
                var p2User = new ApplicationUser
                {
                    UserName = "provider2@softbridge.com",
                    Email = "provider2@softbridge.com",
                    FullName = "Provider 2",
                    UserType = UserType.Provider,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(p2User, "Password123!");
                await userManager.AddToRoleAsync(p2User, UserType.Provider.ToString());

                context.Set<SProvider>().Add(new SProvider
                {
                    Id = Guid.NewGuid(),
                    UserId = p2User.Id,
                    Bio = "UI/UX Designer with 3 years of experience.",
                    Status = ProviderAccountStatus.Approved,
                    TotalReviews = 0,
                    AverageRating = 0
                });
            }

            // 2.(Clients)


            // Client 1:
            if (await userManager.FindByEmailAsync("client1@softbridge.com") == null)
            {
                var c1User = new ApplicationUser
                {
                    UserName = "client1@softbridge.com",
                    Email = "client1@softbridge.com",
                    FullName = "Client 1",
                    UserType = UserType.Client,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(c1User, "Password123!");
                await userManager.AddToRoleAsync(c1User, UserType.Client.ToString());

                context.Set<Client>().Add(new Client
                {
                    Id = Guid.NewGuid(),
                    UserId = c1User.Id
                });
            }

            // Client 2: فرد عادي
            if (await userManager.FindByEmailAsync("client2@softbridge.com") == null)
            {
                var c2User = new ApplicationUser
                {
                    UserName = "client2@softbridge.com",
                    Email = "client2@softbridge.com",
                    FullName = "Client 2",
                    UserType = UserType.Client,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(c2User, "Password123!");
                await userManager.AddToRoleAsync(c2User, UserType.Client.ToString());

                context.Set<Client>().Add(new Client
                {
                    Id = Guid.NewGuid(),
                    UserId = c2User.Id
                });
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedDummyCategoriesAsync(UserManager<ApplicationUser> userManager,ProjectDbContext context)
        {
            // 1.  (Categories)
            var devCategory = await context.Set<Category>().FirstOrDefaultAsync(c => c.Name == "Development");
            if (devCategory == null)
            {
                devCategory = new Category { Id = Guid.NewGuid(), Name = "Development" };
                context.Set<Category>().Add(devCategory);
            }

            var designCategory = await context.Set<Category>().FirstOrDefaultAsync(c => c.Name == "Design");
            if (designCategory == null)
            {
                designCategory = new Category { Id = Guid.NewGuid(), Name = "Design" };
                context.Set<Category>().Add(designCategory);
            }
            await context.SaveChangesAsync();
        }
    }
}
