using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.Service
{
    public class ServiceImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPortfolio { get; set; }
        public int DisplayOrder { get; set; }
    }
}
