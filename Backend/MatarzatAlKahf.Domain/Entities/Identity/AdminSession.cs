using MatarzatAlKahf.Domain.Common;

namespace MatarzatAlKahf.Domain.Entities.Identity;

/// <summary>
/// One issued administrative access token. Administrative JWTs are high privilege, so they are
/// tracked server side: logout, suspension, deletion, and password resets revoke the matching
/// row and the token stops being accepted on the next request.
/// </summary>
public sealed class AdminSession : BaseEntity
{
    public Guid AdminUserId { get; set; }

    public string TokenIdHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public AdminUser AdminUser { get; set; } = null!;
}
