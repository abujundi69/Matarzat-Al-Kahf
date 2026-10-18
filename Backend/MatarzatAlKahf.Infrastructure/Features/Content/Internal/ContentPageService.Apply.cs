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
    private void Apply(ContentPage page, SaveContentPageRequest request)
    {
        page.Type = request.Type;
        page.IsPublished = request.IsPublished;
        foreach (ContentPageTranslationRequest item in request.Translations)
        {
            page.Translations.Add(new ContentPageTranslation { ContentPageId = page.Id, Language = item.Language, Title = item.Title.Trim(), Slug = slugNormalizer.Normalize(item.Slug), Content = item.Content.Trim(), MetaTitle = item.MetaTitle?.Trim() ?? string.Empty, MetaDescription = item.MetaDescription?.Trim() ?? string.Empty });
        }
    }
}
