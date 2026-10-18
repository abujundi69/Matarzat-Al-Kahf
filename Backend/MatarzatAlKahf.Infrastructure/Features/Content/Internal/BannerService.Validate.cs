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
    private static void Validate(SaveBannerRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (request.StartsAtUtc.HasValue && request.EndsAtUtc.HasValue && request.StartsAtUtc >= request.EndsAtUtc)
        {
            errors[nameof(request.EndsAtUtc)] = ["End date must follow start date."];
        }

        if (request.Translations.Select(item => item.Language).Distinct().Count() != 2 || !request.Translations.Any(item => item.Language == LanguageCode.Arabic) || !request.Translations.Any(item => item.Language == LanguageCode.English))
        {
            errors[nameof(request.Translations)] = ["Exactly one Arabic and one English translation are required."];
        }
        else if (request.Translations.Any(item => string.IsNullOrWhiteSpace(item.DesktopImageUrl) || string.IsNullOrWhiteSpace(item.MobileImageUrl) || item.Title.Length > 300 || item.AltText.Length > 500))
        {
            errors[nameof(request.Translations)] = ["Banner translation data is invalid."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Banner data is invalid.", errors);
        }
    }
}
