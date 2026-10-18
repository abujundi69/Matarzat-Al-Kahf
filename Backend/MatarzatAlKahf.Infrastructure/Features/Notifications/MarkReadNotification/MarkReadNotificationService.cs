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
    public async Task MarkReadAsync(Guid id, CancellationToken cancellationToken)
    {
        Guid customerId = RequireCustomer();
        Notification notification = await dbContext.Notifications.SingleOrDefaultAsync(item => item.Id == id && item.CustomerId == customerId && item.Channel == NotificationChannel.InApp && !item.IsDeleted, cancellationToken) ?? throw new NotFoundException("Notification was not found.");
        notification.ReadAtUtc ??= DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
