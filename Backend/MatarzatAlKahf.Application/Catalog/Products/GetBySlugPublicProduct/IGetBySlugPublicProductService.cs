using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface IGetBySlugPublicProductService
{
    Task<PublicProductDetailsDto> GetBySlugAsync(string slug, CancellationToken cancellationToken);
}
