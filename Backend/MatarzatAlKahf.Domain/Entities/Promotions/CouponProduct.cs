using MatarzatAlKahf.Domain.Entities.Catalog;

namespace MatarzatAlKahf.Domain.Entities.Promotions;

public sealed class CouponProduct
{
    public Guid CouponId { get; set; }
    public Guid ProductId { get; set; }
    public bool IsExcluded { get; set; }
    public Coupon Coupon { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
