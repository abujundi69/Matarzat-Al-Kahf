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
    private static AdminCategoryDto MapAdmin(Category category)
    {
        return new AdminCategoryDto(category.Id, category.ParentCategoryId, EmptyToNull(category.ImageUrl), category.SortOrder, category.IsActive, category.Translations.Where(translation => !translation.IsDeleted).OrderBy(translation => translation.Language).Select(translation => new CategoryTranslationDto(translation.Language, translation.Name, translation.Slug, EmptyToNull(translation.Description), EmptyToNull(translation.MetaTitle), EmptyToNull(translation.MetaDescription))).ToList());
    }
}
