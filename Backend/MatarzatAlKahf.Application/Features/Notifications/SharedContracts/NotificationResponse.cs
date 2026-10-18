using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Notifications.DTOs;
public sealed record NotificationResponse(Guid Id, Guid? CustomerId, Guid? AdminUserId, Guid? OrderId, LanguageCode Language, NotificationChannel Channel, NotificationStatus Status, string Title, string Body, DateTime? ReadAtUtc, int RetryCount, string? FailureReason, DateTime CreatedAtUtc);
