using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Identity;

namespace MatarzatAlKahf.Domain.Entities.System;

public sealed class AuditLog : BaseEntity
{
    public Guid? AdminUserId { get; set; }

    // Snapshot of the actor's display name at write time, so the trail stays readable after the admin is renamed or removed.
    public string? ActorName { get; set; }

    public string ActorType { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public Guid? EntityId { get; set; }

    public string Details { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    public string TraceId { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;

    public AdminUser? AdminUser { get; set; }
}
