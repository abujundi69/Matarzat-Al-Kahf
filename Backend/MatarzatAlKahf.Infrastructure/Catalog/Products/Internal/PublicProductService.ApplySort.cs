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
    private static IQueryable<Product> ApplySort(IQueryable<Product> products, string sort, LanguageCode language)
    {
        LanguageCode fallbackLanguage = language == LanguageCode.Arabic ? LanguageCode.English : LanguageCode.Arabic;
        return sort.ToUpperInvariant() switch
        {
            "PRICE-ASC" => products.OrderBy(product => product.Variants.Where(variant => !variant.IsDeleted && variant.IsActive && variant.PriceOverride.HasValue).Select(variant => variant.PriceOverride).Min() ?? product.BasePrice).ThenBy(product => product.Id),
            "PRICE-DESC" => products.OrderByDescending(product => product.Variants.Where(variant => !variant.IsDeleted && variant.IsActive && variant.PriceOverride.HasValue).Select(variant => variant.PriceOverride).Min() ?? product.BasePrice).ThenBy(product => product.Id),
            "NAME" => products.OrderBy(product => product.Translations.Where(translation => !translation.IsDeleted && translation.Language == language).Select(translation => translation.Name).FirstOrDefault() ?? product.Translations.Where(translation => !translation.IsDeleted && translation.Language == fallbackLanguage).Select(translation => translation.Name).FirstOrDefault()).ThenBy(product => product.Id),
            _ => products.OrderByDescending(product => product.PublishedAtUtc ?? product.CreatedAtUtc).ThenBy(product => product.Id)};
    }
}
