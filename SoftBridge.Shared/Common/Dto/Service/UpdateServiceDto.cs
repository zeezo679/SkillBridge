using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.Service
{
    public class UpdateServiceDto
    {
        [Required]
        public Guid CategoryId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(1, 365, ErrorMessage = "Delivery days must be between 1 and 365")]
        public int DeliveryDays { get; set; }
    }
}
