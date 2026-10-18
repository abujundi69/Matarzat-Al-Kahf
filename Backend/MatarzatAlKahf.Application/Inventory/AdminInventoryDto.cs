using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Inventory;
public sealed record AdminInventoryDto(Guid ProductVariantId, Guid ProductId, string ProductName, string Sku, string? Size, string? Color, int QuantityOnHand, int ReservedQuantity, int AvailableQuantity, int ReorderLevel, bool IsLowStock, string RowVersion);
