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
    private static ProductImage CreateImage(CreateProductImageRequest input)
    {
        var image = new ProductImage
        {
            ImageUrl = input.ImageUrl.Trim(),
            ThumbnailUrl = input.ThumbnailUrl.Trim(),
            SortOrder = input.SortOrder,
            IsPrimary = input.IsPrimary
        };
        AddImageTranslations(image, input.Translations);
        return image;
    }
}
