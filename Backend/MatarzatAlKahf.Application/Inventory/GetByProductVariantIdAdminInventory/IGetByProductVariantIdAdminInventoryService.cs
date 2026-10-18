using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Inventory;
public interface IGetByProductVariantIdAdminInventoryService
{
    Task<AdminInventoryDto> GetByProductVariantIdAsync(Guid productVariantId, CancellationToken cancellationToken);
}
