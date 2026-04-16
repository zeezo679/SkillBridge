using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.ServiceRequest.NewDtos
{
    public class CreateRequestDto
    {
        public Guid ServiceId { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal? AgreedPrice { get; set; }
    }
}
