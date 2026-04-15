using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.Notification
{
    public class NotificationContentDto
    {
        public string To { get; set; } // email or id of the user to receive the notification
        public string Subject { get; set; }
        public string Body { get; set; }
        public Guid? ReferenceId { get; set; }
    }
}
