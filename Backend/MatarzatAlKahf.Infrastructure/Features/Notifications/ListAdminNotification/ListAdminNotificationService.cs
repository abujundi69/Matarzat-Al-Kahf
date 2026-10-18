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
    public Task<PagedResult<NotificationResponse>> ListAdminAsync(NotificationQuery query, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(query.Page, query.PageSize);
        IQueryable<Notification> notifications = dbContext.Notifications.Where(item => !item.IsDeleted);
        if (query.Status.HasValue)
        {
            notifications = notifications.Where(item => item.Status == query.Status);
        }

        if (query.Channel.HasValue)
        {
            notifications = notifications.Where(item => item.Channel == query.Channel);
        }

        return PageAsync(notifications, query.Page, query.PageSize, cancellationToken);
    }
}
