using System.Linq.Expressions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.AuditLogs.DTOs;
using MatarzatAlKahf.Application.Features.AuditLogs.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.AuditLogs.Services;
public sealed partial class AuditLogQueryService
{
    public async Task<PagedResult<AuditLogResponse>> ListAsync(AuditLogQuery query, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(query.Page, query.PageSize);
        IQueryable<AuditLog> logs = dbContext.AuditLogs.Where(log => !log.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            string action = query.Action.Trim();
            logs = logs.Where(log => log.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            string entityType = query.EntityType.Trim();
            logs = logs.Where(log => log.EntityName == entityType);
        }

        if (query.ActorId.HasValue)
        {
            logs = logs.Where(log => log.AdminUserId == query.ActorId);
        }

        if (query.FromUtc.HasValue)
        {
            logs = logs.Where(log => log.CreatedAtUtc >= query.FromUtc);
        }

        if (query.ToUtc.HasValue)
        {
            logs = logs.Where(log => log.CreatedAtUtc <= query.ToUtc);
        }

        int count = await logs.CountAsync(cancellationToken);
        List<AuditLogResponse> items = await logs.AsNoTracking().OrderByDescending(log => log.CreatedAtUtc).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).Select(Projection).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(items, query.Page, query.PageSize, count);
    }
}
