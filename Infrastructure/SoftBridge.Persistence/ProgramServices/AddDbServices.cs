
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Persistence.ImplementsContracts.UowImmlementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Persistence.ProgramServices
{
    public static class AddDbServices
    {
        public static IServiceCollection InjectDatabaseService(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<ProjectDbContext>(options =>
               options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
