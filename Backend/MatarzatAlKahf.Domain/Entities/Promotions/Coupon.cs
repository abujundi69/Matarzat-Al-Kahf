using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Promotions;

public sealed class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal? MinimumOrderAmount { get; set; }

    public decimal? MaximumDiscountAmount { get; set; }

    public DateTime? StartsAtUtc { get; set; }

    public DateTime? EndsAtUtc { get; set; }

    public int? TotalUsageLimit { get; set; }

    public int? UsageLimitPerCustomer { get; set; }

    public CouponStatus Status { get; set; }

    public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    public ICollection<CouponCategory> Categories { get; set; } = new List<CouponCategory>();

    public ICollection<CouponProduct> Products { get; set; } = new List<CouponProduct>();
}
