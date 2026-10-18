using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface IGetByIdPublicProductService
{
    Task<PublicProductDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
