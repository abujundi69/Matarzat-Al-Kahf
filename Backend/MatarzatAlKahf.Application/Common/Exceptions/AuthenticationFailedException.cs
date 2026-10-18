namespace MatarzatAlKahf.Application.Common.Exceptions;

public class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException()
        : this("Authentication failed.")
    {
    }

    public AuthenticationFailedException(string message)
        : base(message)
    {
    }

    public AuthenticationFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
