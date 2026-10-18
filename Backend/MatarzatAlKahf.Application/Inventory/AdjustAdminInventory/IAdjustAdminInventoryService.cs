using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Inventory;
public interface IAdjustAdminInventoryService
{
    Task<InventoryAdjustmentResultDto> AdjustAsync(Guid productVariantId, InventoryAdjustmentRequest request, CancellationToken cancellationToken);
}
