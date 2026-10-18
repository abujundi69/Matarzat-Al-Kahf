using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Files.Services;
public sealed partial class ProductImageService
{
    public async Task DeleteAsync(Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        ProductImage image = await dbContext.ProductImages.SingleOrDefaultAsync(item => item.Id == imageId && item.ProductId == productId && !item.IsDeleted, cancellationToken) ?? throw new NotFoundException("Product image was not found.");
        image.IsDeleted = true;
        image.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await fileCleanup.DeleteIfUnreferencedAsync(
            image.ImageUrl,
            cancellationToken);
        if (!string.Equals(
                image.ThumbnailUrl,
                image.ImageUrl,
                StringComparison.OrdinalIgnoreCase))
        {
            await fileCleanup.DeleteIfUnreferencedAsync(
                image.ThumbnailUrl,
                cancellationToken);
        }

        await auditService.RecordAsync("product.image.deleted", nameof(ProductImage), image.Id, $"An image was deleted from product {productId}.", cancellationToken);
    }
}
