using SoftBridge.Abstraction.IServices.Attachement;
using SoftBridge.Abstraction.IServices.Auth;
using SoftBridge.Abstraction.IServices.Category;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Abstraction.IServicesContract.Chat;
using SoftBridge.Abstraction.IServicesContract.Notification;
using SoftBridge.Abstraction.IServicesContract.Review;
using SoftBridge.Abstraction.IServicesContract.Services;
using SoftBridge.Abstraction.IServicesContract.Token;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.DbInitializer;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Persistence.Implements.InitializerImplement;
using SoftBridge.Persistence.ImplementsContracts.UowImmlementation;
using SoftBridge.Services.Resolver;
using SoftBridge.Services.Services;
using SoftBridge.Services.Services.Admin;
using SoftBridge.Services.Services.AuthImplementation;
using SoftBridge.Services.Services.CategoryImplementation;
using SoftBridge.Services.Services.Chat;
using SoftBridge.Services.Services.ClientImplementation;
using SoftBridge.Services.Services.NotificationImplementation;
using SoftBridge.Services.Services.NotificationImplementation.StrategyPattern;
using SoftBridge.Services.Services.ReviewImplementaion;
using SoftBridge.Services.Services.ServiceManagement;
using SoftBridge.Services.Services.ServiceProviderImplementation;
using SoftBridge.Services.Services.Token;
using SoftBridge.Shared.Common.Dto.Notification.Settings;
using SoftBridge.Services.Services.RequestImplementation;
using SoftBridge.Abstraction.IServicesContract.Request;


namespace SoftBridge.Web.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Settings
            services.Configure<EmailSettingsDto>(configuration.GetSection("EmailSettings"));

            // 2. (Strategy Pattern)
            services.AddScoped<INotificationStrategy, EmailNotificationStrategy>();
            services.AddScoped<INotificationStrategy, PushedNotificationStrategy>();
            // Notification Hubs
            services.AddScoped<IWebNotificationPusher, WebNotificationPusher>();

            //3.Services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDbInitializer, DbInitialized>();
            services.AddScoped<IAttachmentService, AttachmentService>();
            services.AddScoped<IServiceManagement, ServiceManagementService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IClientProfileService, ClientService>();
            services.AddScoped<IProviderProfileService, ServiceProviderService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IRequestWorkflowService, RequestService>();

            // 4. transient services : because they are used in resolvers and we want a new instance each time
            // and the class is too small to be scoped or singleton it take string and return string so it is better to be transient
            services.AddTransient(typeof(PictureUrlResolver<,>));

            return services;
        }
    }
}
