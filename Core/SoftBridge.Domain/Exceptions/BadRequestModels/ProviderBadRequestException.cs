using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Domain.Exceptions.BadRequestModels
{
    public class ProviderBadRequestException : BadRequestExceptionCustome
    {
        public ProviderBadRequestException(string message, IEnumerable<string>? errors = null) : base(message, errors)
        {
        }
    }
}
