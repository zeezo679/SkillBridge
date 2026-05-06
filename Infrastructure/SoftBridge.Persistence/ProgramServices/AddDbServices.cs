
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Persistence.ImplementsContracts.UowImmlementation;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace SoftBridge.Persistence.ProgramServices
{
    public static class AddDbServices
    {
        public static IServiceCollection InjectDatabaseService
        (this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            if (env.IsEnvironment("Testing"))
            {
                // Use InMemory for tests — no SQL Server registered at all
                var dbName = configuration["TestDbName"] ?? "CategoryTestDb";
                services.AddDbContext<ProjectDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            }
            else
            {
                services.AddDbContext<ProjectDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            }

            return services;
        }
    }
}
