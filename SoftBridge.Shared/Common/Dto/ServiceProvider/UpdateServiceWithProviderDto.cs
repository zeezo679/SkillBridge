using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.ServiceProvider
{
    public class UpdateServiceWithProviderDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public int DeliveryDays { get; set; }
        public List<IFormFile>? NewImages { get; set; }
        public List<Guid>? RemoveImageIds { get; set; } // IDs of images to be removed from the service gallery, not understand 100%
    }
}
