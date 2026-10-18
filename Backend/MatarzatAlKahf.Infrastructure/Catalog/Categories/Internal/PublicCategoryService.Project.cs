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
    private static IQueryable<CategoryReadRow> Project(IQueryable<Category> query, LanguageCode language)
    {
        return query.Select(category => new CategoryReadRow(category.Id, category.ParentCategoryId, category.ImageUrl, category.SortOrder, category.Products.Count(product => !product.IsDeleted && product.Status == ProductStatus.Active), category.Translations.Where(translation => !translation.IsDeleted).OrderBy(translation => translation.Language == language ? 0 : translation.Language == LanguageCode.Arabic ? 1 : 2).Select(translation => new TranslationReadRow(translation.Name, translation.Slug, translation.Description, translation.MetaTitle, translation.MetaDescription)).First(), category.Children.Where(child => !child.IsDeleted && child.IsActive && child.Translations.Any(translation => !translation.IsDeleted)).OrderBy(child => child.SortOrder).ThenBy(child => child.Id).Select(child => new ChildReadRow(child.Id, child.ParentCategoryId, child.Translations.Where(translation => !translation.IsDeleted).OrderBy(translation => translation.Language == language ? 0 : translation.Language == LanguageCode.Arabic ? 1 : 2).Select(translation => new ChildTranslationReadRow(translation.Name, translation.Slug)).First())).ToList()));
    }
}
