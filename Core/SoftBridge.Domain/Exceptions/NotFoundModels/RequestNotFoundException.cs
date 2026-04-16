using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Domain.Exceptions.NotFoundModels
{
    public class RequestNotFoundException : NotFoundExceptionCustome
    {
        public RequestNotFoundException(string message) : base(message)
        {
        }
    }
}
