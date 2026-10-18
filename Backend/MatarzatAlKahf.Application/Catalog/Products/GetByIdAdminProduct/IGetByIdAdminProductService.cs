using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface IGetByIdAdminProductService
{
    Task<AdminProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
