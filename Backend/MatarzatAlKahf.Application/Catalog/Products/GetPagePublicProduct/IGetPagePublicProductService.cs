using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface IGetPagePublicProductService
{
    Task<PagedResult<PublicProductListItemDto>> GetPageAsync(ProductListQuery query, CancellationToken cancellationToken);
}
