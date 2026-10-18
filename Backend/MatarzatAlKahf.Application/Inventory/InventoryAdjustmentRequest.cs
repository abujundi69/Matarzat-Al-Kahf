using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Inventory;
public sealed record InventoryAdjustmentRequest(int QuantityChange, InventoryTransactionType Type, string? Reference, string? Notes, string RowVersion);
