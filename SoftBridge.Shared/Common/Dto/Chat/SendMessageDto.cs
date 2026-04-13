using System;

namespace SoftBridge.Shared.Common.Dto.Chat;

public class SendMessageDto
{
    public Guid RequestId { get; set; }
    public string Content { get; set; } = string.Empty;
}
