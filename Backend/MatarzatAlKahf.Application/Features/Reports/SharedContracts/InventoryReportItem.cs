using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Reports.DTOs;
public sealed record InventoryReportItem(Guid ProductVariantId, string Sku, string ProductName, int QuantityOnHand, int ReservedQuantity, int AvailableQuantity, int ReorderLevel, bool IsLowStock, bool IsOutOfStock);
