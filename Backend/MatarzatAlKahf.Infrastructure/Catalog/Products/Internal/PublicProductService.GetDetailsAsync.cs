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
    private async Task<PublicProductDetailsDto> GetDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        LanguageCode language = _languageResolver.GetCurrentLanguage();
        ProductDetailsBaseRow? product = await AvailableProducts().Where(entity => entity.Id == id).Select(entity => new ProductDetailsBaseRow(entity.Id, entity.CategoryId, entity.BasePrice, entity.Currency, entity.IsFeatured, entity.PublishedAtUtc)).SingleOrDefaultAsync(cancellationToken);
        if (product is null)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        List<ProductDetailsTranslationRow> productTranslations = await _dbContext.ProductTranslations.AsNoTracking().Where(translation => translation.ProductId == id && !translation.IsDeleted).Select(translation => new ProductDetailsTranslationRow(translation.Language, translation.Name, translation.Slug, translation.ShortDescription, translation.Description, translation.Story, translation.MetaTitle, translation.MetaDescription)).ToListAsync(cancellationToken);
        ProductDetailsTranslationRow selectedProductTranslation = SelectPreferred(productTranslations, language, translation => translation.Language);
        List<CategoryTranslationRow> categoryTranslations = await _dbContext.CategoryTranslations.AsNoTracking().Where(translation => translation.CategoryId == product.CategoryId && !translation.IsDeleted).Select(translation => new CategoryTranslationRow(translation.CategoryId, translation.Language, translation.Name, translation.Slug)).ToListAsync(cancellationToken);
        CategoryTranslationRow selectedCategoryTranslation = SelectPreferred(categoryTranslations, language, translation => translation.Language);
        List<ProductDetailsImageRow> imageRows = await _dbContext.ProductImages.AsNoTracking().Where(image => image.ProductId == id && !image.IsDeleted).OrderByDescending(image => image.IsPrimary).ThenBy(image => image.SortOrder).ThenBy(image => image.Id).Select(image => new ProductDetailsImageRow(image.Id, image.ImageUrl, image.ThumbnailUrl, image.SortOrder, image.IsPrimary)).ToListAsync(cancellationToken);
        Guid[] imageIds = imageRows.Select(image => image.Id).ToArray();
        List<ImageTranslationRow> imageTranslations = imageIds.Length == 0 ? [] : await _dbContext.ProductImageTranslations.AsNoTracking().Where(translation => imageIds.Contains(translation.ProductImageId) && !translation.IsDeleted).Select(translation => new ImageTranslationRow(translation.ProductImageId, translation.Language, translation.AltText)).ToListAsync(cancellationToken);
        List<PublicProductImageDto> images = imageRows.Select(image => new PublicProductImageDto(image.Id, image.ImageUrl, image.ThumbnailUrl, image.SortOrder, image.IsPrimary, SelectOptionalAltText(image.Id, imageTranslations, language))).ToList();
        List<PublicProductVariantDto> variants = await _dbContext.ProductVariants.AsNoTracking().Where(variant => variant.ProductId == id && !variant.IsDeleted && variant.IsActive && !variant.InventoryItem.IsDeleted).OrderBy(variant => variant.Sku).Select(variant => new PublicProductVariantDto(variant.Id, variant.Sku, variant.Size, variant.Color, variant.WeightInGrams, variant.Capacity, variant.PriceOverride ?? product.BasePrice, variant.InventoryItem.QuantityOnHand - variant.InventoryItem.ReservedQuantity > 0, variant.InventoryItem.QuantityOnHand - variant.InventoryItem.ReservedQuantity)).ToListAsync(cancellationToken);
        return new PublicProductDetailsDto(product.Id, selectedProductTranslation.Name, selectedProductTranslation.Slug, EmptyToNull(selectedProductTranslation.ShortDescription), selectedProductTranslation.Description, EmptyToNull(selectedProductTranslation.Story), EmptyToNull(selectedProductTranslation.MetaTitle), EmptyToNull(selectedProductTranslation.MetaDescription), new PublicProductCategoryDto(product.CategoryId, selectedCategoryTranslation.Name, selectedCategoryTranslation.Slug), product.BasePrice, product.Currency, product.IsFeatured, images, variants, product.PublishedAtUtc);
    }
}
