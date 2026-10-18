using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Notifications.Interfaces;
public interface INotificationDeliveryProvider
{
    NotificationChannel Channel { get; }

    bool IsConfigured { get; }

    Task<NotificationDeliveryResult> SendAsync(NotificationDeliveryMessage message, CancellationToken cancellationToken);
}
