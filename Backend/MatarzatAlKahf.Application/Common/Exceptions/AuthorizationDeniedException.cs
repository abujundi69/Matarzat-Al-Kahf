namespace MatarzatAlKahf.Application.Common.Exceptions;

public sealed class AuthorizationDeniedException(string message) : Exception(message);
