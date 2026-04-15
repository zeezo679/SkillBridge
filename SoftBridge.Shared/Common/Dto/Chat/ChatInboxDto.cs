using System;

namespace SoftBridge.Shared.Common.Dto.Chat;

public class ChatInboxDto
{
    public Guid RequestId { get; set; }
    public string RequestTitle { get; set; } = string.Empty;
    public string LastMessageContent { get; set; } = string.Empty;
    public DateTime LastMessageSentAt { get; set; }
    public int UnreadCount { get; set; }
}
