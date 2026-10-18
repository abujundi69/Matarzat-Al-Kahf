using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface IGetSimilarPublicProductService
{
    Task<IReadOnlyList<PublicProductListItemDto>> GetSimilarAsync(Guid id, int count, CancellationToken cancellationToken);
}
