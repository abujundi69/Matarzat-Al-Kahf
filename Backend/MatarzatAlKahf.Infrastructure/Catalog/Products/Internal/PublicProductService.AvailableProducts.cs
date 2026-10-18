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
    private IQueryable<Product> AvailableProducts()
    {
        return _dbContext.Products.AsNoTracking().Where(product => !product.IsDeleted && product.Status == ProductStatus.Active && product.Currency == CurrencyCode.ILS && !product.Category.IsDeleted && product.Category.IsActive && product.Translations.Any(translation => !translation.IsDeleted) && product.Category.Translations.Any(translation => !translation.IsDeleted) && product.Variants.Any(variant => !variant.IsDeleted && variant.IsActive));
    }
}
