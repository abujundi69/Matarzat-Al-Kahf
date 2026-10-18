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
    private IQueryable<Category> AvailableCategories()
    {
        return _dbContext.Categories.AsNoTracking().Where(category => !category.IsDeleted && category.IsActive && category.Translations.Any(translation => !translation.IsDeleted));
    }
}
