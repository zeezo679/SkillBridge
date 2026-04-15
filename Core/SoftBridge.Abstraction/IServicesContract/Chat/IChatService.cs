using System;
using System.Collections.Generic;
using System.Text;
using SoftBridge.Shared.Common.Dto.Chat;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params;

namespace SoftBridge.Abstraction.IServicesContract.Chat
{
    // This interface defines the contract for the private messaging system.
    // It works alongside SignalR Hubs to save messages to the database and retrieve chat histories.
    public interface IChatService
    {
        /// <summary>
        /// Saves a new message to the database.
        /// </summary>
        /// <param name="sendMessageDto">The DTO containing the message details.</param>
        /// <returns>The saved message DTO with its assigned ID and timestamp.</returns>
        Task<MessageDto> SaveMessageAsync(string senderId, SendMessageDto sendMessageDto);

        /// <summary>
        /// Retrieves the chat history for a specific request, paginated.
        /// </summary>
        /// <param name="requestId">The ID of the request to retrieve messages for.</param>
        /// <param name="queryParams">Pagination parameters.</param>
        /// <returns>A paginated response containing the list of messages.</returns>
        Task<PaginationResponse<MessageDto>> GetChatHistoryAsync(string senderId, Guid requestId, BaseQueryParams queryParams); 

        /// <summary>
        /// Marks all messages in a specific chat as read for a given receiver.
        /// </summary>
        /// <param name="requestId">The ID of the request whose messages are to be marked as read.</param>
        /// <param name="receiverId">The ID of the user who is marking the messages as read.</param>
        /// <returns>A boolean indicating whether the operation was successful.</returns>
        Task MarkMessagesAsReadAsync(Guid requestId, string receiverId);

        /// <summary>
        /// Retrieves the list of active chats (inbox) for a specific user.
        /// Each chat includes the last message and the count of unread messages.
        /// </summary> <param name="userId">The ID of the user whose inbox is to be retrieved.</param>
        /// <returns>A list of chat inbox DTOs representing the user's active chats.</returns>
        Task<IEnumerable<ChatInboxDto>> GetUserInboxAsync(string userId); // Get list of active chats
    }
}