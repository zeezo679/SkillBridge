using SoftBridge.Domain.Models.EnumHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Params.Requests
{
    public class RequestQueryParams : BaseQueryParams
    {
        public RequestStatus? Status { get; set; } // Pending, Accepted, Rejected, Completed
        //public Guid? ProviderId { get; set; } // For Admin filtering by specific provider
        //public Guid? ClientId { get; set; } // For Admin filtering by specific client
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
