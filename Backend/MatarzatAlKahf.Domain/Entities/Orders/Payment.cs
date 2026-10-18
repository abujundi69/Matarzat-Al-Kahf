using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Orders;

public sealed class Payment : BaseEntity
{
    public Guid OrderId { get; set; }

    public PaymentMethod Method { get; set; }

    public PaymentStatus Status { get; set; }

    public decimal Amount { get; set; }

    public CurrencyCode Currency { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string TransactionReference { get; set; } = string.Empty;

    public string ProviderResponseReference { get; set; } = string.Empty;

    public DateTime? PaidAtUtc { get; set; }

    public string ProofFilePath { get; set; } = string.Empty;

    public string VerificationNote { get; set; } = string.Empty;

    public Guid? VerifiedByAdminUserId { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public Order Order { get; set; } = null!;

    public AdminUser? VerifiedByAdminUser { get; set; }
}
