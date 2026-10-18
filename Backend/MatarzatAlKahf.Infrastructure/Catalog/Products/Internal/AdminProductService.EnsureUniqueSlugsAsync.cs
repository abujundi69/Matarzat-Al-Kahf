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
    private async Task EnsureUniqueSlugsAsync(IReadOnlyDictionary<LanguageCode, string> slugs, Guid? excludedProductId, CancellationToken cancellationToken)
    {
        foreach ((LanguageCode language, string slug)in slugs)
        {
            bool exists = await _dbContext.ProductTranslations.AnyAsync(translation => translation.Language == language && translation.Slug == slug && (!excludedProductId.HasValue || translation.ProductId != excludedProductId.Value), cancellationToken);
            if (exists)
            {
                throw new ConflictException($"A product slug already exists for language '{language}'.");
            }
        }
    }
}
