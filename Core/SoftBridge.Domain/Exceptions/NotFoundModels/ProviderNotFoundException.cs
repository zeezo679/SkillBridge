using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Domain.Exceptions.NotFoundModels
{
    public class ProviderNotFoundException(string msg) : NotFoundExceptionCustome(msg)
    {
    }
}
