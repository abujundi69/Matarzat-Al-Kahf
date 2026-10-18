namespace MatarzatAlKahf.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : this("One or more validation errors occurred.")
    {
    }

    public ValidationException(string message)
        : base(message)
    {
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ValidationException(
        string message,
        IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; } =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

    public static ValidationException For(string field, string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new ValidationException(
            "One or more validation errors occurred.",
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [field] = [error]
            });
    }
}
