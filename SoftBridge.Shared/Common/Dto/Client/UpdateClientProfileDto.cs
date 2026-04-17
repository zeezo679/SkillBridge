using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftBridge.Shared.Common.Dto.Client
{
    public class UpdateClientProfileDto
    {
        public string FullName { get; set; } = string.Empty;

        
        public IFormFile? ProfileImageUrl { get; set; } // IFormFile
    }
}
