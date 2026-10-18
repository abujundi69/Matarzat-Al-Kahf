namespace MatarzatAlKahf.Application.Common.Exceptions;

public sealed class RateLimitExceededException(string message) : Exception(message);
