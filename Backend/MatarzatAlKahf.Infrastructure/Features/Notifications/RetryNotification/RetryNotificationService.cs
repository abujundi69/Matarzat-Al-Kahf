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
    public async Task RetryAsync(Guid id, CancellationToken cancellationToken)
    {
        Notification notification = await FindAsync(id, cancellationToken);
        if (notification.Status is not (NotificationStatus.Failed or NotificationStatus.Cancelled))
        {
            throw new ConflictException("Only failed or cancelled notifications can be retried.");
        }

        notification.Status = NotificationStatus.Pending;
        notification.FailureReason = string.Empty;
        notification.NextAttemptAtUtc = DateTime.UtcNow;
        notification.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
