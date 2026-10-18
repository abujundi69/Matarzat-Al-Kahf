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
    private static ContentPageResponse MapPublic(ContentPage page, LanguageCode language)
    {
        ContentPageTranslation translation = page.Translations.FirstOrDefault(item => item.Language == language) ?? page.Translations.FirstOrDefault(item => item.Language == LanguageCode.Arabic) ?? page.Translations.First();
        return new ContentPageResponse(page.Id, page.Type, translation.Title, translation.Slug, translation.Content, EmptyToNull(translation.MetaTitle), EmptyToNull(translation.MetaDescription), page.UpdatedAtUtc);
    }
}
