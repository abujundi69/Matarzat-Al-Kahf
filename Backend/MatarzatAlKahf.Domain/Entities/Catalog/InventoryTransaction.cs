using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Catalog;

public sealed class InventoryTransaction : BaseEntity
{
    public Guid InventoryItemId { get; set; }

    public InventoryTransactionType Type { get; set; }

    public int Quantity { get; set; }

    public string? Reference { get; set; }

    public string? Notes { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;
}
