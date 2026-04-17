using SoftBridge.Domain.Models.EnumHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.Service
{
    public class ChangeServiceStatusDto
    {
        public ServiceStatus Status { get; set; }
        public string? RejectionReason { get; set; }
    }
}
