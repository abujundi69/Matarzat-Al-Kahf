using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Infrastructure.Persistence;

namespace MatarzatAlKahf.Infrastructure.Features.AuditLogs.Services;

public sealed class AuditService(
    MatarzatAlKahfDbContext dbContext,
    ICurrentActor currentActor) : IAuditService
{
    public async Task RecordAsync(
        string action,
        string entityType,
        Guid? entityId,
        string summary,
        CancellationToken cancellationToken)
    {
        dbContext.AuditLogs.Add(new AuditLog
        {
            AdminUserId = currentActor.AdminUserId,
            ActorName = string.IsNullOrWhiteSpace(currentActor.ActorName) ? null : currentActor.ActorName,
            ActorType = currentActor.ActorType,
            Action = action,
            EntityName = entityType,
            EntityId = entityId,
            Details = summary,
            TraceId = currentActor.TraceId,
            IpAddress = currentActor.IpAddress,
            UserAgent = string.Empty
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
