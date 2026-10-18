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
    public async Task<IReadOnlyList<BannerResponse>> ListPublicAsync(BannerPosition? position, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        IQueryable<Banner> query = dbContext.Banners.AsNoTracking().Include(banner => banner.Translations).Where(banner => banner.IsActive && !banner.IsDeleted && (!banner.StartsAtUtc.HasValue || banner.StartsAtUtc <= now) && (!banner.EndsAtUtc.HasValue || banner.EndsAtUtc > now));
        if (position.HasValue)
        {
            query = query.Where(banner => banner.Position == position);
        }

        List<Banner> banners = await query.OrderBy(banner => banner.SortOrder).Take(50).ToListAsync(cancellationToken);
        LanguageCode language = languageResolver.GetCurrentLanguage();
        return banners.Select(banner =>
        {
            BannerTranslation translation = SelectTranslation(banner, language);
            return new BannerResponse(banner.Id, banner.Position, banner.SortOrder, translation.Title, translation.DesktopImageUrl, translation.MobileImageUrl, translation.AltText, EmptyToNull(translation.LinkUrl));
        }).ToArray();
    }
}
