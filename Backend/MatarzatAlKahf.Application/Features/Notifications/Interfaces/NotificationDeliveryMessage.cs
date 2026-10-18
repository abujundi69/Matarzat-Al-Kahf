using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Notifications.Interfaces;
public sealed record NotificationDeliveryMessage(Guid NotificationId, string Recipient, string Title, string Body, LanguageCode Language);
