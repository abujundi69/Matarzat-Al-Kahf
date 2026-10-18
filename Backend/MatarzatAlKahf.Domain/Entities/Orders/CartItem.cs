using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Catalog;

namespace MatarzatAlKahf.Domain.Entities.Orders;

public sealed class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public Cart Cart { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}
