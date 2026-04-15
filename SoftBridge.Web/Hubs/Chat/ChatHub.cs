using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SoftBridge.Abstraction.IServicesContract.Chat;
using SoftBridge.Shared.Common.Dto.Chat;

namespace SoftBridge.Web.Hubs.Chat;

[Authorize]
public class ChatHub(IChatService chatService) : Hub
{
    private readonly IChatService _chatService = chatService;
    
    public async Task JoinChat(Guid requestId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, requestId.ToString());
    }

    public async Task SendMessage(Guid requestId, string content)
    {
        var senderId = Context.UserIdentifier!;

        var dto = new SendMessageDto { RequestId = requestId, Content = content };

        var message = await _chatService.SaveMessageAsync(senderId, dto);
        
        await Clients.Group(dto.RequestId.ToString()).SendAsync("ReceiveMessage", message);
    }
}
