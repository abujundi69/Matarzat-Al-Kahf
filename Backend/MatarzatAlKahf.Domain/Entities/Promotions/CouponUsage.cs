using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Entities.Orders;

namespace MatarzatAlKahf.Domain.Entities.Promotions;

public sealed class CouponUsage : BaseEntity
{
    public Guid CouponId { get; set; }

    public Guid OrderId { get; set; }

    public Guid? CustomerId { get; set; }

    public decimal DiscountAmount { get; set; }

    public Coupon Coupon { get; set; } = null!;

    public Order Order { get; set; } = null!;

    public Customer? Customer { get; set; }
}
