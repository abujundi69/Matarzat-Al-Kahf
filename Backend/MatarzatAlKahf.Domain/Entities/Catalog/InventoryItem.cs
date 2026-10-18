using MatarzatAlKahf.Domain.Common;

namespace MatarzatAlKahf.Domain.Entities.Catalog;

public sealed class InventoryItem : BaseEntity
{
    public Guid ProductVariantId { get; set; }

    public int QuantityOnHand { get; set; }

    public int ReservedQuantity { get; set; }

    public int ReorderLevel { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ProductVariant ProductVariant { get; set; } = null!;

    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
