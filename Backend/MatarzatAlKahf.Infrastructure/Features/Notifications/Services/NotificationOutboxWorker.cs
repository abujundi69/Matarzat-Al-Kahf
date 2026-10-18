using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Features.Notifications.Services;

public sealed partial class NotificationOutboxWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationOutboxWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        do
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogWorkerFailure(logger, exception);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MatarzatAlKahfDbContext>();
        Guid[] ids = await dbContext.Notifications.AsNoTracking()
            .Where(notification => notification.Status == NotificationStatus.Pending
                && (!notification.NextAttemptAtUtc.HasValue
                    || notification.NextAttemptAtUtc <= DateTime.UtcNow))
            .OrderBy(notification => notification.CreatedAtUtc)
            .Take(20)
            .Select(notification => notification.Id)
            .ToArrayAsync(cancellationToken);
        foreach (Guid id in ids)
        {
            int claimed = await dbContext.Notifications
                .Where(notification => notification.Id == id
                    && notification.Status == NotificationStatus.Pending)
                .ExecuteUpdateAsync(
                    updates => updates
                        .SetProperty(
                            notification => notification.Status,
                            NotificationStatus.Processing)
                        .SetProperty(
                            notification => notification.UpdatedAtUtc,
                            DateTime.UtcNow),
                    cancellationToken);
            if (claimed == 0)
            {
                continue;
            }

            Notification notification = await dbContext.Notifications
                .SingleAsync(item => item.Id == id, cancellationToken);
            await DeliverAsync(scope.ServiceProvider, notification, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task DeliverAsync(
        IServiceProvider services,
        Notification notification,
        CancellationToken cancellationToken)
    {
        try
        {
            if (notification.Channel == NotificationChannel.InApp)
            {
                notification.Status = NotificationStatus.Sent;
                notification.SentAtUtc = DateTime.UtcNow;
                notification.NextAttemptAtUtc = null;
                return;
            }

            INotificationDeliveryProvider? provider = services
                .GetServices<INotificationDeliveryProvider>()
                .SingleOrDefault(item => item.Channel == notification.Channel);
            if (provider is null || !provider.IsConfigured)
            {
                throw new InvalidOperationException("Notification provider is not configured.");
            }

            NotificationDeliveryResult result = await provider.SendAsync(
                new NotificationDeliveryMessage(
                    notification.Id,
                    notification.Recipient,
                    notification.Title,
                    notification.Body,
                    notification.Language),
                cancellationToken);
            notification.ProviderReference = result.ProviderReference;
            notification.Status = NotificationStatus.Sent;
            notification.SentAtUtc = DateTime.UtcNow;
            notification.NextAttemptAtUtc = null;
            notification.FailureReason = string.Empty;
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            notification.RetryCount++;
            notification.FailureReason = "Delivery provider is unavailable or rejected the message.";
            notification.Status = notification.RetryCount >= 5
                ? NotificationStatus.Failed
                : NotificationStatus.Pending;
            notification.NextAttemptAtUtc = notification.Status == NotificationStatus.Pending
                ? DateTime.UtcNow.AddMinutes(Math.Pow(2, notification.RetryCount))
                : null;
        }
    }

    [LoggerMessage(
        EventId = 4100,
        Level = LogLevel.Error,
        Message = "Notification outbox worker iteration failed")]
    private static partial void LogWorkerFailure(ILogger logger, Exception exception);
}
