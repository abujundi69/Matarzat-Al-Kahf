namespace MatarzatAlKahf.Domain.Entities.Identity;

public sealed class AdminUserRole
{
    public Guid AdminUserId { get; set; }

    public Guid RoleId { get; set; }

    public AdminUser AdminUser { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
