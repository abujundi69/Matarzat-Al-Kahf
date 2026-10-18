namespace MatarzatAlKahf.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException()
        : this("The requested resource was not found.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotFoundException(string resourceName, object key)
        : this($"{resourceName} with identifier '{key}' was not found.")
    {
    }
}
