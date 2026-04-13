using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Domain.Exceptions
{
    public class UnauthorizedExceptionCusotme(string message = "Invalid Operation") 
        : Exception(message) 
    {
    }
}
