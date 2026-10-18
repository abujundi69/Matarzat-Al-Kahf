using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Inventory;
public sealed record InventoryAdjustmentResultDto(Guid ProductVariantId, int QuantityOnHand, int ReservedQuantity, int AvailableQuantity, int ReorderLevel, bool IsLowStock, string RowVersion);
