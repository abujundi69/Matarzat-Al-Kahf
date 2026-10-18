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
    private async Task<IReadOnlyList<SocialLinkResponse>> ListAsync(bool publicOnly, CancellationToken cancellationToken)
    {
        IQueryable<SocialMediaLink> query = dbContext.SocialMediaLinks.AsNoTracking().Where(link => !link.IsDeleted);
        if (publicOnly)
        {
            query = query.Where(link => link.IsActive);
        }

        return await query.OrderBy(link => link.SortOrder).Select(link => new SocialLinkResponse(link.Id, link.Platform, link.Url, link.IsActive, link.SortOrder)).ToListAsync(cancellationToken);
    }
}
