using System;

namespace SoftBridge.Shared.Common.Dto.Chat;

public class MessageDto
{
    public Guid Id { get; set; }
    public string SenderId { get; set; } = null!;
    public string SenderName { get; set; } = string.Empty;
    public Guid RequestId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}
