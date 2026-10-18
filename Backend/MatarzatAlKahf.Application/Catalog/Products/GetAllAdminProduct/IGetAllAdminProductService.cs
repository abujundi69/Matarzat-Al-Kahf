using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface IGetAllAdminProductService
{
    Task<PagedResult<AdminProductDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
}
