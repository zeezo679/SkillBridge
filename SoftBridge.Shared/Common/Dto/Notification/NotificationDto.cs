using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.Notification
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } 
        public Guid? ReferenceId { get; set; } // Link for the frontend to navigate to the relevant page (e.g., order details, chat message, etc.)
    }
}
