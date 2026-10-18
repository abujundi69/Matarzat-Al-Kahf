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
    private static void UpdateImages(Product product, IReadOnlyList<UpdateProductImageRequest> inputs)
    {
        var submittedIds = inputs.Where(input => input.Id.HasValue).Select(input => input.Id!.Value).ToHashSet();
        foreach (ProductImage existing in product.Images.Where(image => !image.IsDeleted))
        {
            if (!submittedIds.Contains(existing.Id))
            {
                existing.IsDeleted = true;
                existing.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        foreach (UpdateProductImageRequest input in inputs)
        {
            if (!input.Id.HasValue)
            {
                product.Images.Add(CreateImage(input));
                continue;
            }

            ProductImage image = product.Images.SingleOrDefault(entity => entity.Id == input.Id.Value && !entity.IsDeleted) ?? throw new NotFoundException(nameof(ProductImage), input.Id.Value);
            image.ImageUrl = input.ImageUrl.Trim();
            image.ThumbnailUrl = input.ThumbnailUrl.Trim();
            image.SortOrder = input.SortOrder;
            image.IsPrimary = input.IsPrimary;
            image.UpdatedAtUtc = DateTime.UtcNow;
            UpdateImageTranslations(image, input.Translations);
        }
    }
}
