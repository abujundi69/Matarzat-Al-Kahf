namespace MatarzatAlKahf.Application.Common.Exceptions;

public sealed class ServiceUnavailableException(string message) : Exception(message);
