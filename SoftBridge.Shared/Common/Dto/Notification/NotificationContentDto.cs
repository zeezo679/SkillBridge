using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.Notification
{
    public class NotificationContentDto
    {
        public string UserId { get; set; } = string.Empty; // For DB and Push (SignalR)
        public string? Email { get; set; } // For Email Strategy (Nullable because not all notifications need emails)
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Guid? ReferenceId { get; set; }
    }
}
