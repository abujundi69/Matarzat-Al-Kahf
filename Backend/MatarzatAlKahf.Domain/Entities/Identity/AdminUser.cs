using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Identity;

public sealed class AdminUser : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string NormalizedPhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public UserStatus Status { get; set; }

    public LanguageCode PreferredLanguage { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public ICollection<AdminUserRole> UserRoles { get; set; } = new List<AdminUserRole>();

    public ICollection<AdminSession> Sessions { get; set; } = new List<AdminSession>();

    public ICollection<OrderStatusHistory> OrderStatusChanges { get; set; } = new List<OrderStatusHistory>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
