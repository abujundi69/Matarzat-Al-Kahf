using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Notifications.DTOs;
public sealed record EnqueueNotificationRequest(Guid? CustomerId, Guid? AdminUserId, Guid? OrderId, LanguageCode Language, NotificationChannel Channel, string Title, string Body, string Recipient);
