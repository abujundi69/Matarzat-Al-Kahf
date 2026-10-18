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
    private static BannerTranslation SelectTranslation(Banner banner, LanguageCode language) => banner.Translations.FirstOrDefault(item => item.Language == language) ?? banner.Translations.FirstOrDefault(item => item.Language == LanguageCode.Arabic) ?? banner.Translations.First();
}
