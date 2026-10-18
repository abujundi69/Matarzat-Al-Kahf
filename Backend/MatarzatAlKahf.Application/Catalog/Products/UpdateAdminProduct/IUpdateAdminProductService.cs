using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface IUpdateAdminProductService
{
    Task<AdminProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
}
