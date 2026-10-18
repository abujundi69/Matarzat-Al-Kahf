using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Inventory;
public interface IGetAllAdminInventoryService
{
    Task<PagedResult<AdminInventoryDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
}
