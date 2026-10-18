using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Infrastructure.Features.Notifications.Services;

public sealed class DisabledNotificationProvider(
    NotificationChannel channel) : INotificationDeliveryProvider
{
    public NotificationChannel Channel { get; } = channel;
    public bool IsConfigured => false;

    public Task<NotificationDeliveryResult> SendAsync(
        NotificationDeliveryMessage message,
        CancellationToken cancellationToken) =>
        throw new ServiceUnavailableException(
            $"{Channel} notification delivery is not configured.");
}
