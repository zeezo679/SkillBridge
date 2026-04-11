using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Domain.Exceptions.NotFoundModels
{
    public class ProviderNotFoundException : NotFoundExceptionCustome
    {
        public ProviderNotFoundException(string message) : base(message)
        {
        }
    }
}
