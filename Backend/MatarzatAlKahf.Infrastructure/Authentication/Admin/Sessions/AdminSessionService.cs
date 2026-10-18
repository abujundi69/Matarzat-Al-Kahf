using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Authentication.Admin;

public sealed class AdminSessionService(MatarzatAlKahfDbContext dbContext) : IAdminSessionService
{
    private readonly MatarzatAlKahfDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task LogoutAsync(string tokenId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tokenId))
        {
            return;
        }

        string hash = AdminSessionTokenHasher.Hash(tokenId);
        AdminSession? session = await _dbContext.AdminSessions.SingleOrDefaultAsync(
            candidate => candidate.TokenIdHash == hash && candidate.RevokedAtUtc == null,
            cancellationToken);
        if (session is null)
        {
            return;
        }

        session.RevokedAtUtc = DateTime.UtcNow;
        session.UpdatedAtUtc = session.RevokedAtUtc;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeOthersAsync(
        Guid adminUserId,
        string currentTokenId,
        CancellationToken cancellationToken)
    {
        string? keepHash = string.IsNullOrWhiteSpace(currentTokenId)
            ? null
            : AdminSessionTokenHasher.Hash(currentTokenId);
        DateTime now = DateTime.UtcNow;
        await _dbContext.AdminSessions
            .Where(session => session.AdminUserId == adminUserId
                && session.RevokedAtUtc == null
                && (keepHash == null || session.TokenIdHash != keepHash))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(session => session.RevokedAtUtc, now)
                    .SetProperty(session => session.UpdatedAtUtc, now),
                cancellationToken);
    }

    public async Task RevokeAllAsync(Guid adminUserId, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        await _dbContext.AdminSessions
            .Where(session => session.AdminUserId == adminUserId && session.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(session => session.RevokedAtUtc, now)
                    .SetProperty(session => session.UpdatedAtUtc, now),
                cancellationToken);
    }
}
