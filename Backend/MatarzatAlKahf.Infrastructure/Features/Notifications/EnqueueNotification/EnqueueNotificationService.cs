using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Notifications.Services;
public sealed partial class NotificationService
{
    public async Task<Guid> EnqueueAsync(EnqueueNotificationRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var notification = new Notification
        {
            CustomerId = request.CustomerId,
            AdminUserId = request.AdminUserId,
            OrderId = request.OrderId,
            Language = request.Language,
            Channel = request.Channel,
            Status = NotificationStatus.Pending,
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            Recipient = request.Recipient.Trim(),
            ProviderReference = string.Empty,
            FailureReason = string.Empty,
            NextAttemptAtUtc = DateTime.UtcNow
        };
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync(cancellationToken);
        return notification.Id;
    }
}
