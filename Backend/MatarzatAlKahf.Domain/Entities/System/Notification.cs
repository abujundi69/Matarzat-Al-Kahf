using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.System;

public sealed class Notification : BaseEntity
{
    public Guid? CustomerId { get; set; }

    public Guid? AdminUserId { get; set; }

    public LanguageCode Language { get; set; }

    public NotificationChannel Channel { get; set; }

    public NotificationStatus Status { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string Recipient { get; set; } = string.Empty;

    public string ProviderReference { get; set; } = string.Empty;

    public string FailureReason { get; set; } = string.Empty;

    public DateTime? SentAtUtc { get; set; }

    public DateTime? ReadAtUtc { get; set; }

    public Guid? OrderId { get; set; }

    public int RetryCount { get; set; }

    public DateTime? NextAttemptAtUtc { get; set; }

    public Customer? Customer { get; set; }

    public AdminUser? AdminUser { get; set; }
}
