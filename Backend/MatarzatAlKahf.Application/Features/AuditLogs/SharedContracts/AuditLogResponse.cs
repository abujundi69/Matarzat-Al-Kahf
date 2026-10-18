namespace MatarzatAlKahf.Application.Features.AuditLogs.DTOs;
public sealed record AuditLogResponse(Guid Id, Guid? ActorId, string ActorName, string ActorType, string Action, string EntityType, Guid? EntityId, string Summary, string TraceId, string? IpAddress, DateTime CreatedAtUtc);
