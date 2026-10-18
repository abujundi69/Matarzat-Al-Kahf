using MatarzatAlKahf.Application.Common.Exceptions;

namespace MatarzatAlKahf.Application.Common.Pagination;

public static class PagedResultFactory
{
    public static PagedResult<T> Create<T>(
        IReadOnlyList<T> items,
        int page,
        int pageSize,
        int totalItems)
    {
        ArgumentNullException.ThrowIfNull(items);
        PaginationRules.Validate(page, pageSize);

        if (totalItems < 0)
        {
            throw ValidationException.For(nameof(totalItems), "Total items cannot be negative.");
        }

        int totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<T>(items, page, pageSize, totalItems, totalPages);
    }
}
