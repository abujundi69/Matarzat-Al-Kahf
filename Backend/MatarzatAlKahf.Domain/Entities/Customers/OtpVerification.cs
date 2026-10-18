using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Customers;

public sealed class OtpVerification : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;

    public string CodeHash { get; set; } = string.Empty;

    public OtpPurpose Purpose { get; set; }

    public OtpStatus Status { get; set; }

    public LanguageCode Language { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public DateTime? ConsumedAtUtc { get; set; }

    public DateTime LastSentAtUtc { get; set; }

    public int AttemptCount { get; set; }

    public int MaximumAttempts { get; set; }

    public string VerificationTokenHash { get; set; } = string.Empty;
}
