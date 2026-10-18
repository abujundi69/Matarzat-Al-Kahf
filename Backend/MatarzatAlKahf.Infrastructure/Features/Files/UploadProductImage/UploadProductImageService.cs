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
    public async Task<FileResponse> UploadAsync(Guid productId, ProductImageUploadRequest request, CancellationToken cancellationToken)
    {
        Product product = await dbContext.Products.Include(item => item.Images).SingleOrDefaultAsync(item => item.Id == productId && !item.IsDeleted, cancellationToken) ?? throw new NotFoundException("Product was not found.");
        if (request.ArabicAltText?.Length > 500 || request.EnglishAltText?.Length > 500)
        {
            throw ValidationException.For(nameof(request), "Alternative text is too long.");
        }

        FileResponse file = await fileService.UploadAsync(request.File with { IsPublic = true }, cancellationToken);
        if (request.IsPrimary)
        {
            foreach (ProductImage existing in product.Images)
            {
                existing.IsPrimary = false;
            }
        }

        var image = new ProductImage
        {
            ProductId = productId,
            ImageUrl = file.Url,
            ThumbnailUrl = file.Url,
            SortOrder = request.SortOrder,
            IsPrimary = request.IsPrimary || product.Images.Count == 0,
            Translations = [new ProductImageTranslation
            {
                Language = LanguageCode.Arabic,
                AltText = request.ArabicAltText?.Trim() ?? string.Empty
            }, new ProductImageTranslation
            {
                Language = LanguageCode.English,
                AltText = request.EnglishAltText?.Trim() ?? string.Empty
            }

            ]
        };
        product.Images.Add(image);
        dbContext.ProductImages.Add(image);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            dbContext.ChangeTracker.Clear();
            await fileCleanup.DeleteIfUnreferencedAsync(
                file.Url,
                CancellationToken.None);
            throw;
        }

        await auditService.RecordAsync("product.image.uploaded", nameof(ProductImage), image.Id, $"An image was uploaded for product {productId}.", cancellationToken);
        return file;
    }
}
