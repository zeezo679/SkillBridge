
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using SoftBridge.Domain.Models.User;
using SoftBridge.Persistence;

namespace SoftBridge.Web.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection InjectIdentityCore(this IServiceCollection services)
        {
            // Add Identity services
            // Add Identity services
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                // for forget password 
                // to van used => var otp = await _userManager.GeneratePasswordResetTokenAsync(user);
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
            })
                 .AddRoles<IdentityRole>()
                 .AddEntityFrameworkStores<ProjectDbContext>()
                 .AddDefaultTokenProviders();
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(5);
            });
            return services;
        }
        public static IServiceCollection InjectRateLimiting(this IServiceCollection services)
        {
            // Add rate limiting services
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("OtpPolicy", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(2); // time to expire is 2 minutes 
                    opt.PermitLimit = 3; // numbers to try is 3 times
                    opt.QueueLimit = 0; // no queue, requests will be rejected immediately when the limit is reached
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });
            return services;
        }
    }
}
