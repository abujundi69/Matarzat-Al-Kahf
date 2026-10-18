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
    private static PublicCategoryDto Map(CategoryReadRow row)
    {
        return new PublicCategoryDto(row.Id, row.ParentCategoryId, EmptyToNull(row.ImageUrl), row.SortOrder, row.Translation.Name, row.Translation.Slug, EmptyToNull(row.Translation.Description), EmptyToNull(row.Translation.MetaTitle), EmptyToNull(row.Translation.MetaDescription), row.ProductCount, row.Children.Select(child => new PublicCategorySummaryDto(child.Id, child.ParentCategoryId, child.Translation.Name, child.Translation.Slug)).ToList());
    }
}
