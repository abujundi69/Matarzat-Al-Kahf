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
    public async Task<AdminBannerResponse> UpdateAsync(Guid id, SaveBannerRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        Banner banner = await FindAsync(id, true, cancellationToken);
        string[] previousImageUrls = banner.Translations
            .SelectMany(translation => new[]
            {
                translation.DesktopImageUrl,
                translation.MobileImageUrl
            })
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        dbContext.BannerTranslations.RemoveRange(banner.Translations);
        banner.Translations.Clear();
        Apply(banner, request);
        banner.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (string previousImageUrl in previousImageUrls)
        {
            await fileCleanup.DeleteIfUnreferencedAsync(
                previousImageUrl,
                cancellationToken);
        }

        await AuditAsync("banner.updated", banner, cancellationToken);
        return MapAdmin(banner);
    }
}
