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
    private static AdminBannerResponse MapAdmin(Banner banner) => new(banner.Id, banner.Position, banner.SortOrder, banner.IsActive, banner.StartsAtUtc, banner.EndsAtUtc, banner.Translations.OrderBy(item => item.Language).Select(item => new BannerTranslationRequest(item.Language, item.Title, item.DesktopImageUrl, item.MobileImageUrl, item.AltText, EmptyToNull(item.LinkUrl))).ToArray());
}
