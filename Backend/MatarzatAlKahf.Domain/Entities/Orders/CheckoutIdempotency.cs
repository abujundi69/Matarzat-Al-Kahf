using MatarzatAlKahf.Domain.Common;

namespace MatarzatAlKahf.Domain.Entities.Orders;

public sealed class CheckoutIdempotency : BaseEntity
{
    public string Scope { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public Order? Order { get; set; }
}
