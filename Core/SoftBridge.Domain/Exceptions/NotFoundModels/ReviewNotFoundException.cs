using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Domain.Exceptions.NotFoundModels
{
    public class ReviewNotFoundException : NotFoundExceptionCustome
    {
        public ReviewNotFoundException(string message) : base(message)
        {
        }
    }
}
