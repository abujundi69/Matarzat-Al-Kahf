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
    private static IQueryable<Product> ApplyFilters(IQueryable<Product> products, ProductListQuery query, LanguageCode language)
    {
        if (query.CategoryId.HasValue)
        {
            products = products.Where(product => product.CategoryId == query.CategoryId.Value);
        }

        if (query.IsFeatured.HasValue)
        {
            products = products.Where(product => product.IsFeatured == query.IsFeatured.Value);
        }

        if (query.InStock.HasValue)
        {
            products = query.InStock.Value ? products.Where(product => product.Variants.Any(variant => !variant.IsDeleted && variant.IsActive && !variant.InventoryItem.IsDeleted && variant.InventoryItem.QuantityOnHand - variant.InventoryItem.ReservedQuantity > 0)) : products.Where(product => !product.Variants.Any(variant => !variant.IsDeleted && variant.IsActive && !variant.InventoryItem.IsDeleted && variant.InventoryItem.QuantityOnHand - variant.InventoryItem.ReservedQuantity > 0));
        }

        if (!string.IsNullOrWhiteSpace(query.Size))
        {
            string size = query.Size.Trim();
            products = products.Where(product => product.Variants.Any(variant => !variant.IsDeleted && variant.IsActive && variant.Size == size));
        }

        if (!string.IsNullOrWhiteSpace(query.Color))
        {
            string color = query.Color.Trim();
            products = products.Where(product => product.Variants.Any(variant => !variant.IsDeleted && variant.IsActive && variant.Color == color));
        }

        if (query.HasDiscount.HasValue)
        {
            products = query.HasDiscount.Value ? products.Where(product => product.Variants.Any(variant => !variant.IsDeleted && variant.IsActive && variant.PriceOverride.HasValue && variant.PriceOverride.Value < product.BasePrice)) : products.Where(product => !product.Variants.Any(variant => !variant.IsDeleted && variant.IsActive && variant.PriceOverride.HasValue && variant.PriceOverride.Value < product.BasePrice));
        }

        if (query.MinPrice.HasValue)
        {
            decimal minimumPrice = query.MinPrice.Value;
            products = products.Where(product => product.BasePrice >= minimumPrice && !product.Variants.Any(variant => !variant.IsDeleted && variant.IsActive && variant.PriceOverride.HasValue && variant.PriceOverride.Value < minimumPrice));
        }

        if (query.MaxPrice.HasValue)
        {
            decimal maximumPrice = query.MaxPrice.Value;
            products = products.Where(product => product.BasePrice <= maximumPrice || product.Variants.Any(variant => !variant.IsDeleted && variant.IsActive && variant.PriceOverride.HasValue && variant.PriceOverride.Value <= maximumPrice));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();
            LanguageCode fallbackLanguage = language == LanguageCode.Arabic ? LanguageCode.English : LanguageCode.Arabic;
            products = products.Where(product => product.Translations.Any(translation => !translation.IsDeleted && translation.Language == language && (translation.Name.Contains(search) || translation.ShortDescription.Contains(search) || translation.Description.Contains(search) || translation.Story.Contains(search))) || (!product.Translations.Any(translation => !translation.IsDeleted && translation.Language == language) && product.Translations.Any(translation => !translation.IsDeleted && translation.Language == fallbackLanguage && (translation.Name.Contains(search) || translation.ShortDescription.Contains(search) || translation.Description.Contains(search) || translation.Story.Contains(search)))));
        }

        return products;
    }
}
