namespace MatarzatAlKahf.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException()
        : this("The requested operation conflicts with the current resource state.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
