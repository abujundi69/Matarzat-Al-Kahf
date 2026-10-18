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
    public async Task<SocialLinkResponse> CreateAsync(SaveSocialLinkRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var link = new SocialMediaLink();
        Apply(link, request);
        dbContext.SocialMediaLinks.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("social.link.created", link, cancellationToken);
        return Map(link);
    }
}
