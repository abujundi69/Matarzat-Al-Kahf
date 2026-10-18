using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Orders;

public sealed class Cart : BaseEntity
{
    public Guid? CustomerId { get; set; }
    public string GuestTokenHash { get; set; } = string.Empty;
    public Guid? CouponId { get; set; }
    public CartStatus Status { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Customer? Customer { get; set; }
    public Coupon? Coupon { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
