using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoftBridge.Abstraction.IServicesContract.Review;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Domain.Models.Shared;
using SoftBridge.Persistence;
using SoftBridge.Persistence.ProgramServices;
using SoftBridge.Services.AutoMapper;
using SoftBridge.Services.Services.ReviewImplementaion;
using SoftBridge.Web.Extensions;
using SoftBridge.Web.Hubs.Chat;
using SoftBridge.Web.Hubs.Notification;
using SoftBridge.Web.Middleware;
namespace SoftBridge.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // get from presistence layer (database configure) 
            builder.Services.InjectDatabaseService(builder.Configuration);
            // get from identity layer in web project (Identity core configure)
            builder.Services.InjectIdentityCore();
            // inject all application services 
            builder.Services.AddApplicationServices(builder.Configuration);
            // get from web layer (rate limiting configure)
            builder.Services.InjectRateLimiting();
            // get from services layer
            builder.Services.InjectAutoMapperService();
            //Add SignalR
            builder.Services.AddSignalR();


            builder.Services.AddScoped<IReviewService, ReviewService>();

            // add AppDbContext Service
            // builder.Services.AddDbContext<ProjectDbContext>(options =>
            // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            // );

            // add identity core
            builder.Services.AddDataProtection();
            
                   
            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            #region ToBuildSwaggerUI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                #region ToBuildSwaggerUI
                app.UseSwagger();
                app.UseSwaggerUI();
                #endregion
            }

            app.UseMiddleware<GlobalErrorHandlerMiddleware>();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.MapHub<NotificationHub>("/notificationHub");
            app.MapHub<ChatHub>("/chatHub");

            // CORS MUST be between UseRouting and UseAuth
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();
            app.MapControllers();

            app.Run();
        }
    }
}
