using SoftBridge.Persistence.Extensions;
using SoftBridge.Persistence.ProgramServices;
using SoftBridge.Services.AutoMapper;
using SoftBridge.Web.Extensions;
using SoftBridge.Web.Hubs.Chat;
using SoftBridge.Web.Hubs.Notification;
using SoftBridge.Web.Middleware;

namespace SoftBridge.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Database & Infrastructure
            builder.Services.InjectDatabaseService(builder.Configuration);

            // 2. Identity & Security
            builder.Services.InjectIdentityCore();
            builder.Services.InjectRateLimiting();
            builder.Services.AddDataProtection();
            // Add JWT Authentication and CORS configuration
            builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
            builder.Services.AddAuthorization();
            builder.Services.AddCustomCors(builder.Configuration);

            // 3. Application Services & AutoMapper
            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.InjectAutoMapperService();

            // 4. API Core Features & SignalR
            builder.Services.AddControllers();
            builder.Services.AddSignalR();

            // 5. Swagger & OpenAPI Documentation
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Initialization
            await app.SeedDatabaseAsync();

            // Configure the HTTP Request Pipeline
            app.UseMiddleware<GlobalErrorHandlerMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseRateLimiter();

            // CORS MUST be placed between UseRouting and UseAuthentication
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            // ==========================================
            // Endpoints Mapping
            // ==========================================
            app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");
            app.MapHub<ChatHub>("/chatHub");

            await app.RunAsync();
        }
    }
}