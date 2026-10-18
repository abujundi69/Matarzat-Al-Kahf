using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Catalog.Products;
public sealed partial class AdminProductService
{
    private static void UpdateImageTranslations(ProductImage image, IEnumerable<ProductImageTranslationRequest> inputs)
    {
        foreach (ProductImageTranslationRequest input in inputs)
        {
            ProductImageTranslation? translation = image.Translations.SingleOrDefault(entity => entity.Language == input.Language);
            if (translation is null)
            {
                image.Translations.Add(new ProductImageTranslation { Language = input.Language, AltText = input.AltText?.Trim() ?? string.Empty });
                continue;
            }

            translation.AltText = input.AltText?.Trim() ?? string.Empty;
            translation.IsDeleted = false;
            translation.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
