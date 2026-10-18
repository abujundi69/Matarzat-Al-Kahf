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
    public async Task<IReadOnlyList<PublicCategoryDto>> GetAllAsync(Guid? parentCategoryId, CancellationToken cancellationToken)
    {
        IQueryable<Category> query = AvailableCategories();
        if (parentCategoryId.HasValue)
        {
            query = query.Where(category => category.ParentCategoryId == parentCategoryId);
        }

        LanguageCode language = _languageResolver.GetCurrentLanguage();
        query = query.OrderBy(category => category.SortOrder).ThenBy(category => category.Id);
        List<CategoryReadRow> rows = await Project(query, language).ToListAsync(cancellationToken);
        return rows.Select(Map).ToList();
    }
}
