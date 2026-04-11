using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.ServiceProvider
{
    public class RespondToRequestDto
    {
        public decimal? AgreedPrice { get; set; }
        public string? RejectionReason { get; set; }
    }
}
