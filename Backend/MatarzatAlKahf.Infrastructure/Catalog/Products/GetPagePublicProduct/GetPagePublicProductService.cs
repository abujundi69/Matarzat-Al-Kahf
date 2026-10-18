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
    public async Task<PagedResult<PublicProductListItemDto>> GetPageAsync(ProductListQuery query, CancellationToken cancellationToken)
    {
        _validator.Validate(query);
        LanguageCode language = _languageResolver.GetCurrentLanguage();
        IQueryable<Product> products = ApplyFilters(AvailableProducts(), query, language);
        int totalItems = await products.CountAsync(cancellationToken);
        List<ProductListBaseRow> productRows = await ApplySort(products, query.Sort, language).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).Select(product => new ProductListBaseRow(product.Id, product.CategoryId, product.BasePrice, product.Variants.Where(variant => !variant.IsDeleted && variant.IsActive && variant.PriceOverride.HasValue).Select(variant => variant.PriceOverride).Min() ?? product.BasePrice, product.Currency, product.IsFeatured, product.PublishedAtUtc, product.Variants.Any(variant => !variant.IsDeleted && variant.IsActive && !variant.InventoryItem.IsDeleted && variant.InventoryItem.QuantityOnHand - variant.InventoryItem.ReservedQuantity > 0))).ToListAsync(cancellationToken);
        if (productRows.Count == 0)
        {
            return PagedResultFactory.Create(Array.Empty<PublicProductListItemDto>(), query.Page, query.PageSize, totalItems);
        }

        Guid[] productIds = productRows.Select(row => row.Id).ToArray();
        Guid[] categoryIds = productRows.Select(row => row.CategoryId).Distinct().ToArray();
        List<ProductListTranslationRow> productTranslations = await _dbContext.ProductTranslations.AsNoTracking().Where(translation => productIds.Contains(translation.ProductId) && !translation.IsDeleted).Select(translation => new ProductListTranslationRow(translation.ProductId, translation.Language, translation.Name, translation.Slug)).ToListAsync(cancellationToken);
        List<CategoryTranslationRow> categoryTranslations = await _dbContext.CategoryTranslations.AsNoTracking().Where(translation => categoryIds.Contains(translation.CategoryId) && !translation.IsDeleted).Select(translation => new CategoryTranslationRow(translation.CategoryId, translation.Language, translation.Name, translation.Slug)).ToListAsync(cancellationToken);
        List<ProductListImageRow> images = await _dbContext.ProductImages.AsNoTracking().Where(image => productIds.Contains(image.ProductId) && !image.IsDeleted).Select(image => new ProductListImageRow(image.Id, image.ProductId, image.ImageUrl, image.IsPrimary, image.SortOrder)).ToListAsync(cancellationToken);
        Guid[] imageIds = images.Select(image => image.Id).ToArray();
        List<ImageTranslationRow> imageTranslations = imageIds.Length == 0 ? [] : await _dbContext.ProductImageTranslations.AsNoTracking().Where(translation => imageIds.Contains(translation.ProductImageId) && !translation.IsDeleted).Select(translation => new ImageTranslationRow(translation.ProductImageId, translation.Language, translation.AltText)).ToListAsync(cancellationToken);
        List<PublicProductListItemDto> items = productRows.Select(row => MapListItem(row, language, productTranslations, categoryTranslations, images, imageTranslations)).ToList();
        return PagedResultFactory.Create(items, query.Page, query.PageSize, totalItems);
    }
}
