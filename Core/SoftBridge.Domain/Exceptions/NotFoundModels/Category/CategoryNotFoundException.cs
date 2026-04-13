using System;

namespace SoftBridge.Domain.Exceptions.NotFoundModels.Category;

public class CategoryNotFoundException : NotFoundExceptionCustome
{
    public CategoryNotFoundException(string message) : base(message) {}
}
