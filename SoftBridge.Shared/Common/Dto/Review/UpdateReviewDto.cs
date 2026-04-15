using System;
using System.Collections.Generic;
using System.Text;

namespace E_commerce.Shared.Common.Dto.Review
{
    public class UpdateReviewDto
    {
        public byte Rating { get; set; }
        public string? Comment { get; set; }
    }
}
