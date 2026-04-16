using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.ServiceRequest.NewDtos
{
    public class RequestDto
    {
        public Guid Id { get; set; }

        // Service info
        public Guid ServiceId { get; set; }
        public string ServiceTitle { get; set; } = string.Empty;

        // Client Info
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;

        // Provider Info
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public decimal? AgreedPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
