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
    public Task<PagedResult<NotificationResponse>> ListCustomerAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        Guid id = RequireCustomer();
        return PageAsync(dbContext.Notifications.Where(notification => notification.CustomerId == id && notification.Channel == NotificationChannel.InApp && !notification.IsDeleted), page, pageSize, cancellationToken);
    }
}
