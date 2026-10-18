namespace MatarzatAlKahf.Application.Authentication.Admin;

public interface IAdminSessionService
{
    /// <summary>Revokes the single session behind the supplied token identifier.</summary>
    Task LogoutAsync(string tokenId, CancellationToken cancellationToken);

    /// <summary>
    /// Revokes every live session for an administrator. Used when access is withdrawn or the
    /// password changes, so an already-issued token stops working immediately.
    /// </summary>
    Task RevokeAllAsync(Guid adminUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Revokes every live session for an administrator except the one identified by
    /// <paramref name="currentTokenId"/>. Used after a self-service password change so other
    /// devices lose access without signing the caller out of the request they are making.
    /// </summary>
    Task RevokeOthersAsync(
        Guid adminUserId,
        string currentTokenId,
        CancellationToken cancellationToken);
}
