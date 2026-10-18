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
    private static void Apply(Banner banner, SaveBannerRequest request)
    {
        banner.Position = request.Position;
        banner.SortOrder = request.SortOrder;
        banner.IsActive = request.IsActive;
        banner.StartsAtUtc = request.StartsAtUtc;
        banner.EndsAtUtc = request.EndsAtUtc;
        foreach (BannerTranslationRequest item in request.Translations)
        {
            banner.Translations.Add(new BannerTranslation { BannerId = banner.Id, Language = item.Language, Title = item.Title.Trim(), DesktopImageUrl = item.DesktopImageUrl.Trim(), MobileImageUrl = item.MobileImageUrl.Trim(), AltText = item.AltText.Trim(), LinkUrl = item.LinkUrl?.Trim() ?? string.Empty });
        }
    }
}
