using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Params.Admin
{
    public class ClientQueryParams : BaseQueryParams
    {
        public bool? IsActive { get; set; } // Filter by active/banned clients
    }
}
