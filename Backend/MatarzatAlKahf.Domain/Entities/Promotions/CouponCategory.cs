using MatarzatAlKahf.Domain.Entities.Catalog;

namespace MatarzatAlKahf.Domain.Entities.Promotions;

public sealed class CouponCategory
{
    public Guid CouponId { get; set; }
    public Guid CategoryId { get; set; }
    public Coupon Coupon { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
