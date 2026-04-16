using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Domain.Exceptions.BadRequestModels
{
    public class RequestBadRequestException : BadRequestExceptionCustome
    {
        public RequestBadRequestException(string message, IEnumerable<string>? errors = null) : base(message, errors)
        {
        }
    }
}
