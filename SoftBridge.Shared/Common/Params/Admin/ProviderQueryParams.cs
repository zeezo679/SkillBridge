using SoftBridge.Domain.Models.EnumHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Params.Admin
{
    public class ProviderQueryParams : BaseQueryParams
    {
        
        // 1. Status Filter: To get Pending, Approved, or Rejected providers
        public ProviderAccountStatus? Status { get; set; }

        // 2. Rating Filter: To fetch top-rated providers (e.g., >= 4.0)
        public float? MinRating { get; set; }

        // 3. Date Range Filters: To get providers registered in a specific month/year
        public DateTime? RegisteredFrom { get; set; }
        public DateTime? RegisteredTo { get; set; }
    }
}
