using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Inventory;
public interface IGetLowStockAdminInventoryService
{
    Task<PagedResult<AdminInventoryDto>> GetLowStockAsync(int page, int pageSize, CancellationToken cancellationToken);
}
