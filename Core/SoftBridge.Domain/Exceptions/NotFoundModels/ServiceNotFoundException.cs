using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Domain.Exceptions.NotFoundModels
{
    public class ServiceNotFoundException(string message) : NotFoundExceptionCustome(message)
    {
    }
}
