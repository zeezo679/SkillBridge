using System;
using System.Collections.Generic;
using System.Text;

namespace E_commerce.Shared.Common.Dto.Service
{
    // Used when viewing a single service in full detail
    public class ServiceDetailsDto
    {
        public Guid Id { get; set; }
        public Guid ProviderId { get; set; }
        public Guid CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DeliveryDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public float AverageRating { get; set; }

        // all image Slider
        public List<ServiceImageDto> Images { get; set; } = new List<ServiceImageDto>();
    }
}
