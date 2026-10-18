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
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Product product = await _dbContext.Products
            .Include(entity => entity.Variants)
            .Include(entity => entity.Images)
            .SingleOrDefaultAsync(entity => entity.Id == id && !entity.IsDeleted, cancellationToken) ?? throw new NotFoundException(nameof(Product), id);
        string[] imageUrls = product.Images
            .Where(image => !image.IsDeleted)
            .SelectMany(image => new[] { image.ImageUrl, image.ThumbnailUrl })
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        DateTime now = DateTime.UtcNow;
        product.IsDeleted = true;
        product.Status = ProductStatus.Discontinued;
        product.UpdatedAtUtc = now;
        foreach (ProductVariant variant in product.Variants)
        {
            variant.IsActive = false;
            variant.UpdatedAtUtc = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        foreach (string imageUrl in imageUrls)
        {
            await _fileCleanup.DeleteIfUnreferencedAsync(
                imageUrl,
                cancellationToken);
        }

        LogProductDeleted(_logger, id, null);
    }
}
