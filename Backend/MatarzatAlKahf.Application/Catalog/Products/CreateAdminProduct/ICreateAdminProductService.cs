using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface ICreateAdminProductService
{
    Task<AdminProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
}
