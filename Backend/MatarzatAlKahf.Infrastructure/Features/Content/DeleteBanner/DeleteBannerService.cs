using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Application.Features.Banners.Interfaces;
using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;
public sealed partial class BannerService
{
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Banner banner = await FindAsync(id, true, cancellationToken);
        string[] imageUrls = banner.Translations
            .SelectMany(translation => new[]
            {
                translation.DesktopImageUrl,
                translation.MobileImageUrl
            })
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        banner.IsDeleted = true;
        banner.IsActive = false;
        banner.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (string imageUrl in imageUrls)
        {
            await fileCleanup.DeleteIfUnreferencedAsync(
                imageUrl,
                cancellationToken);
        }

        await AuditAsync("banner.deleted", banner, cancellationToken);
    }
}
