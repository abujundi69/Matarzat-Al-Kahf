namespace MatarzatAlKahf.Application.Features.AuditLogs.DTOs;
public sealed record AuditLogQuery(int Page = 1, int PageSize = 20, string? Action = null, string? EntityType = null, Guid? ActorId = null, DateTime? FromUtc = null, DateTime? ToUtc = null);
