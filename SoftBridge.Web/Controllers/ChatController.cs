using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftBridge.Abstraction.IServicesContract.Chat;
using SoftBridge.Shared.Common.Dto.Chat;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params;

namespace SoftBridge.Web.Controllers;

[Authorize(Roles = "Client,Provider")]
public class ChatController(IChatService chatService) : AppBaseController
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("inbox")]
    public async Task<IActionResult> Inbox()
    {
        var userId = CurrentUserId;

        var result = await chatService.GetUserInboxAsync(userId);

        return Success(result, "Inbox retrieved successfully");
    }

    [HttpGet("{requestId}/history")]
    public async Task<IActionResult> History([FromRoute] Guid requestId, [FromQuery] BaseQueryParams queryParams)
    {
        var senderId = CurrentUserId;

        var result = await chatService.GetChatHistoryAsync(senderId, requestId, queryParams);

        return Success<PaginationResponse<MessageDto>>(result, "Chat history retrieved successfully");
    }

    [HttpPut("{requestId}/read")]
    public async Task<IActionResult> MarkAsRead([FromRoute] Guid requestId)
    {
        var receiverId = CurrentUserId;

        await chatService.MarkMessagesAsReadAsync(requestId, receiverId);

        return Success("Messages marked as read successfully");
    }
}
