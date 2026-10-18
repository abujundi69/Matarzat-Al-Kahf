using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;
using MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;
public sealed partial class SocialLinkService
{
    public async Task<SocialLinkResponse> UpdateAsync(Guid id, SaveSocialLinkRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        SocialMediaLink link = await FindAsync(id, true, cancellationToken);
        Apply(link, request);
        link.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("social.link.updated", link, cancellationToken);
        return Map(link);
    }
}
