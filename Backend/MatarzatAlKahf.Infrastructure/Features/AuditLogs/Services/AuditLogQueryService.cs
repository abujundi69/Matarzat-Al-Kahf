using System.Linq.Expressions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.AuditLogs.DTOs;
using MatarzatAlKahf.Application.Features.AuditLogs.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.AuditLogs.Services;
public sealed partial class AuditLogQueryService(MatarzatAlKahfDbContext dbContext) : IAuditLogQueryService
{
    // Prefers the name snapshot recorded at write time; falls back to a live lookup for rows written before ActorName existed.
    private static Expression<Func<AuditLog, AuditLogResponse>> Projection { get; } = log => new(
        log.Id,
        log.AdminUserId,
        !string.IsNullOrWhiteSpace(log.ActorName)
            ? log.ActorName
            : log.AdminUser == null
                ? "UnknownUser"
                : log.AdminUser.IsDeleted
                    ? "DeletedUser"
                    : log.AdminUser.FullName,
        log.ActorType,
        log.Action,
        log.EntityName,
        log.EntityId,
        log.Details,
        log.TraceId,
        log.IpAddress == string.Empty ? null : log.IpAddress,
        log.CreatedAtUtc);
}
