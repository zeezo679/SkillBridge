using System;
using AutoMapper;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Domain.Models.Shared;
using SoftBridge.Shared.Common.Dto.Chat;

namespace SoftBridge.Services.AutoMapper.ChatProfile;

public class ChatMappingProfile : Profile
{
    public ChatMappingProfile()
    {
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.FullName));


        //Refactor later if needed
        // CreateMap<ServiceRequest, ChatInboxDto>()
        //     .ForMember(dest => dest.RequestTitle, 
        //         opt => opt.MapFrom(src => src.Service.Title))
        //     .ForMember(dest => dest.LastMessageContent, 
        //         opt => opt.MapFrom(src => src.Messages
        //             .OrderByDescending(m => m.SentAt)
        //             .FirstOrDefault() != null 
        //                 ? src.Messages.OrderByDescending(m => m.SentAt).First().Content 
        //                 : string.Empty))
        //     .ForMember(dest => dest.LastMessageSentAt, 
        //         opt => opt.MapFrom(src => src.Messages
        //             .OrderByDescending(m => m.SentAt)
        //             .FirstOrDefault() != null
        //                 ? src.Messages.OrderByDescending(m => m.SentAt).First().SentAt
        //                 : DateTime.MinValue))
        //     .ForMember(dest => dest.UnreadCount, 
        //         opt => opt.MapFrom<ChatInboxUnreadCountResolver>());
    }
}
