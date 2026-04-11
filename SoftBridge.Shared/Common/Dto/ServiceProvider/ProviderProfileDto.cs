using SoftBridge.Domain.Models.EnumHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.ServiceProvider
{
    public class ProviderProfileDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string? Bio { get; set; }

        [Url]
        public string? ProfileImageUrl { get; set; }

        [Url]
        public string? CvUrl { get; set; }

        [Url]
        public string? PortfolioLink { get; set; }
        public string AccountStatus { get; set; } = string.Empty;
        public float AverageRating { get; set; } = 0f;
        public int TotalReviews { get; set; }
        public DateTime ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }




    }
}
