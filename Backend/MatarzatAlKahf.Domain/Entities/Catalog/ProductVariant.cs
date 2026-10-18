using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Orders;

namespace MatarzatAlKahf.Domain.Entities.Catalog;

public sealed class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string? Size { get; set; }

    public string? Color { get; set; }

    public decimal? WeightInGrams { get; set; }

    public string? Capacity { get; set; }

    public decimal? PriceOverride { get; set; }

    public bool IsActive { get; set; }

    public Product Product { get; set; } = null!;

    public InventoryItem InventoryItem { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
