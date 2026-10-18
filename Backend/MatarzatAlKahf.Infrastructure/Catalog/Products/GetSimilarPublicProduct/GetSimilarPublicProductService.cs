using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Catalog.Products;
public sealed partial class PublicProductService
{
    public async Task<IReadOnlyList<PublicProductListItemDto>> GetSimilarAsync(Guid id, int count, CancellationToken cancellationToken)
    {
        if (count is < 1 or > 24)
        {
            throw ValidationException.For(nameof(count), "Count must be between 1 and 24.");
        }

        PublicProductDetailsDto product = await GetDetailsAsync(id, cancellationToken);
        PagedResult<PublicProductListItemDto> candidates = await GetPageAsync(new ProductListQuery(PageSize: count + 1, CategoryId: product.Category.Id, InStock: true), cancellationToken);
        return candidates.Items.Where(item => item.Id != id).Take(count).ToArray();
    }
}
