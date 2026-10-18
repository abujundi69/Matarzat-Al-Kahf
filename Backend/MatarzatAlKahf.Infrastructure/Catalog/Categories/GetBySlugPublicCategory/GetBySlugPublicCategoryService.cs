using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Catalog.Categories;
public sealed partial class PublicCategoryService
{
    public async Task<PublicCategoryDto> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        string normalizedSlug = _slugNormalizer.Normalize(slug);
        LanguageCode language = _languageResolver.GetCurrentLanguage();
        Guid? categoryId = await _dbContext.CategoryTranslations.AsNoTracking().Where(translation => !translation.IsDeleted && translation.Slug == normalizedSlug && !translation.Category.IsDeleted && translation.Category.IsActive).OrderBy(translation => translation.Language == language ? 0 : translation.Language == LanguageCode.Arabic ? 1 : 2).Select(translation => (Guid? )translation.CategoryId).FirstOrDefaultAsync(cancellationToken);
        if (!categoryId.HasValue)
        {
            throw new NotFoundException("Category slug", normalizedSlug);
        }

        return await GetByIdAsync(categoryId.Value, cancellationToken);
    }
}
