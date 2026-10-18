namespace MatarzatAlKahf.Application.Common.Exceptions;

public sealed class ConcurrencyConflictException : ConflictException
{
    public ConcurrencyConflictException()
        : base("The resource was changed by another request. Reload it and try again.")
    {
    }

    public ConcurrencyConflictException(string message)
        : base(message)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
