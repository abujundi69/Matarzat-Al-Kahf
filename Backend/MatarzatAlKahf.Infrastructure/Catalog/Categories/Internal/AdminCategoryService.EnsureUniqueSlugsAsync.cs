using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Catalog.Categories;
public sealed partial class AdminCategoryService
{
    private async Task EnsureUniqueSlugsAsync(IReadOnlyDictionary<LanguageCode, string> slugs, Guid? excludedCategoryId, CancellationToken cancellationToken)
    {
        foreach ((LanguageCode language, string slug)in slugs)
        {
            bool exists = await _dbContext.CategoryTranslations.AnyAsync(translation => translation.Language == language && translation.Slug == slug && (!excludedCategoryId.HasValue || translation.CategoryId != excludedCategoryId.Value), cancellationToken);
            if (exists)
            {
                throw new ConflictException($"A category slug already exists for language '{language}'.");
            }
        }
    }
}
