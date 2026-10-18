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
    public async Task CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        Notification notification = await FindAsync(id, cancellationToken);
        if (notification.Status == NotificationStatus.Sent)
        {
            throw new ConflictException("A sent notification cannot be cancelled.");
        }

        notification.Status = NotificationStatus.Cancelled;
        notification.NextAttemptAtUtc = null;
        notification.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
