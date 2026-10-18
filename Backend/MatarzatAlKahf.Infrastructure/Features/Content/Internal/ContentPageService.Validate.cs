using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;
public sealed partial class ContentPageService
{
    private static void Validate(SaveContentPageRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (request.Translations.Select(item => item.Language).Distinct().Count() != 2 || !request.Translations.Any(item => item.Language == LanguageCode.Arabic) || !request.Translations.Any(item => item.Language == LanguageCode.English))
        {
            errors[nameof(request.Translations)] = ["Exactly one Arabic and one English translation are required."];
        }
        else if (request.Translations.Any(item => string.IsNullOrWhiteSpace(item.Title) || string.IsNullOrWhiteSpace(item.Slug) || string.IsNullOrWhiteSpace(item.Content) || item.Title.Length > 300 || item.Slug.Length > 300 || item.MetaTitle?.Length > 300 || item.MetaDescription?.Length > 1000))
        {
            errors[nameof(request.Translations)] = ["Content translation data is invalid."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Content page data is invalid.", errors);
        }
    }
}
