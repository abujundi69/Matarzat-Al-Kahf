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
    public async Task<IReadOnlyList<ContentPageResponse>> ListPublicAsync(ContentPageType? type, CancellationToken cancellationToken)
    {
        IQueryable<ContentPage> query = dbContext.ContentPages.AsNoTracking().Include(page => page.Translations).Where(page => page.IsPublished && !page.IsDeleted);
        if (type.HasValue)
        {
            query = query.Where(page => page.Type == type);
        }

        List<ContentPage> pages = await query.OrderBy(page => page.Type).Take(100).ToListAsync(cancellationToken);
        LanguageCode language = languageResolver.GetCurrentLanguage();
        return pages.Select(page => MapPublic(page, language)).ToArray();
    }
}
