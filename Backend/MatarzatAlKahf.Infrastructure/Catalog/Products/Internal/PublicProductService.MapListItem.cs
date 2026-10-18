using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Catalog.Products;
public sealed partial class PublicProductService
{
    private static PublicProductListItemDto MapListItem(ProductListBaseRow row, LanguageCode language, List<ProductListTranslationRow> productTranslations, List<CategoryTranslationRow> categoryTranslations, List<ProductListImageRow> images, List<ImageTranslationRow> imageTranslations)
    {
        ProductListTranslationRow productTranslation = SelectPreferred(productTranslations.Where(translation => translation.ProductId == row.Id), language, translation => translation.Language);
        CategoryTranslationRow categoryTranslation = SelectPreferred(categoryTranslations.Where(translation => translation.CategoryId == row.CategoryId), language, translation => translation.Language);
        ProductListImageRow? primaryImage = images.Where(image => image.ProductId == row.Id).OrderByDescending(image => image.IsPrimary).ThenBy(image => image.SortOrder).ThenBy(image => image.Id).FirstOrDefault();
        return new PublicProductListItemDto(row.Id, productTranslation.Name, productTranslation.Slug, new PublicProductCategoryDto(row.CategoryId, categoryTranslation.Name, categoryTranslation.Slug), row.BasePrice, row.MinimumEffectivePrice, row.Currency, row.IsFeatured, primaryImage?.ImageUrl, primaryImage is null ? null : SelectOptionalAltText(primaryImage.Id, imageTranslations, language), row.InStock, row.PublishedAtUtc);
    }
}
