using MatarzatAlKahf.Domain.Common;

namespace MatarzatAlKahf.Domain.Entities.Customers;

public sealed class CustomerSession : BaseEntity
{
    public Guid CustomerId { get; set; }

    public string TokenIdHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public Customer Customer { get; set; } = null!;
}
