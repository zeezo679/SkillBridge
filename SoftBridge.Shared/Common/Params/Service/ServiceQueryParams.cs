using SoftBridge.Domain.Models.EnumHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Params.Service
{
    public class ServiceQueryParams : BaseQueryParams
    {
        public Guid? CategoryId { get; set; } // Filter by Category
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public ServiceStatus? Status { get; set; } // Pending, Approved, Rejected
    }
}
