using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Domain.Exceptions.NotFoundModels
{
    internal class ReviewBadRequestException : BadRequestExceptionCustome
    {
        public ReviewBadRequestException(string message, IEnumerable<string>? errors = null) : base(message, errors)
        {
        }
    }
}
