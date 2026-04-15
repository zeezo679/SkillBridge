using Microsoft.Extensions.DependencyInjection;
using SoftBridge.Services.AutoMapper.AuthMapping;
using SoftBridge.Services.AutoMapper.ClientProfile;
using System;
using System.Collections.Generic;
using System.Text;

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
                cfg.AddProfile(new ReviewMappingProfile());
                cfg.AddProfile(new ProviderMappingProfile());
                cfg.AddProfile(new ClientMappingProfile());
            });
            return services;
        }
    }
}
