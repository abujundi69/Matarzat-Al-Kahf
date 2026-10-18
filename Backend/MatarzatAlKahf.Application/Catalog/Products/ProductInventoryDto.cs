using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record ProductInventoryDto(int QuantityOnHand, int ReservedQuantity, int AvailableQuantity, int ReorderLevel, bool IsLowStock, string RowVersion);
