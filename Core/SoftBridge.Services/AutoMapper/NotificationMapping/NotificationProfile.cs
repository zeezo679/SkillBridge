using AutoMapper;
using SoftBridge.Domain.Models.Shared;
using SoftBridge.Shared.Common.Dto.Notification;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.AutoMapper.NotificationMapping
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationDto>();
        }
    }
}
