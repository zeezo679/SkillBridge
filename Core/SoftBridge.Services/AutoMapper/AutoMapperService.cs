
using Microsoft.Extensions.DependencyInjection;
using SoftBridge.Services.AutoMapper.AuthMapping;
using SoftBridge.Services.AutoMapper.ClientProfile;
using SoftBridge.Services.AutoMapper.ProviderProfile;
using SoftBridge.Services.AutoMapper.RequestProfile;

using SoftBridge.Services.AutoMapper.ServiceProfile;
using SoftBridge.Services.AutoMapper.ChatProfile;
using System;
using System.Collections.Generic;
using System.Text;
using SoftBridge.Services.AutoMapper.CategoryProfile;
using SoftBridge.Services.AutoMapper.ServiceRequestMappingProfile;
using SoftBridge.Services.AutoMapper.NotificationMapping;
using SoftBridge.Services.AutoMapper.ReviewProfile;

namespace SoftBridge.Services.AutoMapper
{
    public static class AutoMapperService
    {
        public static IServiceCollection InjectAutoMapperService(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                //cfg.AddProfile(new [Auth]Profile());
                cfg.AddProfile(new AuthProfile());
                cfg.AddProfile(new ProviderMappingProfile());
                cfg.AddProfile(new ClientMappingProfile());
                cfg.AddProfile(new RequestMappingProfile());
                cfg.AddProfile(new ChatMappingProfile());
                cfg.AddProfile(new ServiceMappingProfile());
                cfg.AddProfile(new CategoryMappingProfile());
                cfg.AddProfile(new ServiceRequestProfile());
                cfg.AddProfile(new NotificationProfile());
                cfg.AddProfile(new ReviewMappingProfile());
            });
            return services;
        }
    }
}
