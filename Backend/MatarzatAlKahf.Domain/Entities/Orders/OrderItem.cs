using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Catalog;

namespace MatarzatAlKahf.Domain.Entities.Orders;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariantId { get; set; }

    public string ProductNameSnapshot { get; set; } = string.Empty;

    public string SkuSnapshot { get; set; } = string.Empty;

    public string VariantDescriptionSnapshot { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public ProductVariant? ProductVariant { get; set; }
}
