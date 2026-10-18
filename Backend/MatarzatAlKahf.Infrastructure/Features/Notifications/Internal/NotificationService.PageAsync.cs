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
    private static async Task<PagedResult<NotificationResponse>> PageAsync(IQueryable<Notification> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(page, pageSize);
        int count = await query.CountAsync(cancellationToken);
        List<NotificationResponse> items = await query.AsNoTracking().OrderByDescending(item => item.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).Select(item => new NotificationResponse(item.Id, item.CustomerId, item.AdminUserId, item.OrderId, item.Language, item.Channel, item.Status, item.Title, item.Body, item.ReadAtUtc, item.RetryCount, item.FailureReason == string.Empty ? null : item.FailureReason, item.CreatedAtUtc)).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(items, page, pageSize, count);
    }
}
