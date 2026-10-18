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
    public async Task MarkAllReadAsync(CancellationToken cancellationToken)
    {
        Guid customerId = RequireCustomer();
        await dbContext.Notifications.Where(item => item.CustomerId == customerId && item.Channel == NotificationChannel.InApp && item.ReadAtUtc == null && !item.IsDeleted).ExecuteUpdateAsync(updates => updates.SetProperty(item => item.ReadAtUtc, DateTime.UtcNow), cancellationToken);
    }
}
