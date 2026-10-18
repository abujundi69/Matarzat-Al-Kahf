using MatarzatAlKahf.Application.Common.Exceptions;

namespace MatarzatAlKahf.Application.Common.Pagination;

public static class PaginationRules
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;

    public static void Validate(int page, int pageSize)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (page < 1)
        {
            errors[nameof(page)] = ["Page must be greater than or equal to 1."];
        }

        if (pageSize < 1 || pageSize > MaximumPageSize)
        {
            errors[nameof(pageSize)] =
            [$"Page size must be between 1 and {MaximumPageSize}."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Pagination is invalid.", errors);
        }
    }
}
