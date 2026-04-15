using System;

namespace SoftBridge.Domain.Exceptions.NotFoundModels;

public class SenderNotFoundException(string message) : NotFoundExceptionCustome(message) {}
