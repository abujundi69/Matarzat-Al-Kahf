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
    public async Task<ContentPageResponse> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        string normalized = slugNormalizer.Normalize(slug);
        LanguageCode language = languageResolver.GetCurrentLanguage();
        ContentPage? page = await dbContext.ContentPages.AsNoTracking().Include(item => item.Translations).SingleOrDefaultAsync(item => item.IsPublished && !item.IsDeleted && item.Translations.Any(translation => translation.Language == language && translation.Slug == normalized), cancellationToken);
        return page is null ? throw new NotFoundException("Content page was not found.") : MapPublic(page, language);
    }
}
