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
    private async Task EnsureUniqueSkusAsync(IEnumerable<SkuCandidate> candidates, CancellationToken cancellationToken)
    {
        foreach (SkuCandidate candidate in candidates)
        {
            bool exists = await _dbContext.ProductVariants.AnyAsync(variant => variant.Sku == candidate.NormalizedSku && (!candidate.Id.HasValue || variant.Id != candidate.Id.Value), cancellationToken);
            if (exists)
            {
                throw new ConflictException($"SKU '{candidate.NormalizedSku}' already exists.");
            }
        }
    }
}
