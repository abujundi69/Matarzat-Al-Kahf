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
    public async Task<PublicProductDetailsDto> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        string normalizedSlug = _slugNormalizer.Normalize(slug);
        LanguageCode language = _languageResolver.GetCurrentLanguage();
        Guid? productId = await _dbContext.ProductTranslations.AsNoTracking().Where(translation => !translation.IsDeleted && translation.Slug == normalizedSlug && !translation.Product.IsDeleted && translation.Product.Status == ProductStatus.Active && !translation.Product.Category.IsDeleted && translation.Product.Category.IsActive).OrderBy(translation => translation.Language == language ? 0 : translation.Language == LanguageCode.Arabic ? 1 : 2).Select(translation => (Guid? )translation.ProductId).FirstOrDefaultAsync(cancellationToken);
        if (!productId.HasValue)
        {
            throw new NotFoundException("Product slug", normalizedSlug);
        }

        return await GetDetailsAsync(productId.Value, cancellationToken);
    }
}
