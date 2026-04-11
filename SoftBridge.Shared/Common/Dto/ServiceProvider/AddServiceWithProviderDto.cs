using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.ServiceProvider
{
    public class AddServiceWithProviderDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public int DeliveryDays { get; set; }

        // portfolio images uploaded with the service
        public List<IFormFile>? Images { get; set; }
    }
}
