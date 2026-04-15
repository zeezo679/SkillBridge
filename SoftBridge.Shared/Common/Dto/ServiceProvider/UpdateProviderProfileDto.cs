using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.ServiceProvider
{
    public class UpdateProviderProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Bio  { get; set; }
        public string? PortfolioLink { get; set; }
        public IFormFile? ProfilePicture { get; set; }
        public IFormFile? Cv { get; set; }
    }
}
