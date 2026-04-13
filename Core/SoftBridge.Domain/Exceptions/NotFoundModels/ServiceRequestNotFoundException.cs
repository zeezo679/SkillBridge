using System;

namespace SoftBridge.Domain.Exceptions.NotFoundModels;

public class ServiceRequestNotFoundException(string message) : NotFoundExceptionCustome(message) {}
