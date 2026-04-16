using SoftBridge.Shared.Common.Dto.ServiceRequest;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.ServiceRequest.NewDtos
{
    // full details — used for single request view
    // includes review if one exists
    public class RequestDetailsDto
    {
        public Guid Id { get; set; }

        // Service Info
        public Guid ServiceId { get; set; }
        public string ServiceTitle { get; set; } = string.Empty;
        public string ServiceCategory { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }

        // Client Info
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;

        // Provider Info
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderEmail { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public decimal? AgreedPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // null if review not yet submitted
        public ReviewSummaryDto? Review { get; set; }
    }
}
