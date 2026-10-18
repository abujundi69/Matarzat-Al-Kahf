using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Catalog.Products;
public sealed partial class AdminProductService(MatarzatAlKahfDbContext dbContext, IProductRequestValidator validator, ISlugNormalizer slugNormalizer, ISkuNormalizer skuNormalizer, IUploadedFileCleanupService fileCleanup, ILogger<AdminProductService> logger) : IAdminProductService
{
    private static readonly Action<ILogger, Guid, Exception?> LogProductCreated = LoggerMessage.Define<Guid>(LogLevel.Information, new EventId(3001, nameof(LogProductCreated)), "Product {ProductId} was created");
    private static readonly Action<ILogger, Guid, Exception?> LogProductUpdated = LoggerMessage.Define<Guid>(LogLevel.Information, new EventId(3002, nameof(LogProductUpdated)), "Product {ProductId} was updated");
    private static readonly Action<ILogger, Guid, Exception?> LogProductDeleted = LoggerMessage.Define<Guid>(LogLevel.Information, new EventId(3003, nameof(LogProductDeleted)), "Product {ProductId} was soft deleted");
    private readonly MatarzatAlKahfDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IProductRequestValidator _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ISlugNormalizer _slugNormalizer = slugNormalizer ?? throw new ArgumentNullException(nameof(slugNormalizer));
    private readonly ISkuNormalizer _skuNormalizer = skuNormalizer ?? throw new ArgumentNullException(nameof(skuNormalizer));
    private readonly IUploadedFileCleanupService _fileCleanup = fileCleanup ?? throw new ArgumentNullException(nameof(fileCleanup));
    private readonly ILogger<AdminProductService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private sealed record SkuCandidate(Guid? Id, string NormalizedSku);
}

internal static class ProductDtoMapper
{
    public static AdminProductDto MapAdmin(Product product)
    {
        return new AdminProductDto(product.Id, product.CategoryId, product.BasePrice, product.Currency, product.Status, product.IsFeatured, product.PublishedAtUtc, product.Translations.Where(translation => !translation.IsDeleted).OrderBy(translation => translation.Language).Select(translation => new ProductTranslationDto(translation.Language, translation.Name, translation.Slug, EmptyToNull(translation.ShortDescription), translation.Description, EmptyToNull(translation.Story), EmptyToNull(translation.MetaTitle), EmptyToNull(translation.MetaDescription))).ToList(), product.Images.Where(image => !image.IsDeleted).OrderBy(image => image.SortOrder).ThenBy(image => image.Id).Select(image => new AdminProductImageDto(image.Id, image.ImageUrl, image.ThumbnailUrl, image.SortOrder, image.IsPrimary, image.Translations.Where(translation => !translation.IsDeleted).OrderBy(translation => translation.Language).Select(translation => new ProductImageTranslationDto(translation.Language, EmptyToNull(translation.AltText))).ToList())).ToList(), product.Variants.Where(variant => !variant.IsDeleted).OrderBy(variant => variant.Sku).Select(MapVariant).ToList());
    }

    private static AdminProductVariantDto MapVariant(ProductVariant variant)
    {
        InventoryItem inventory = variant.InventoryItem;
        int availableQuantity = inventory.QuantityOnHand - inventory.ReservedQuantity;
        return new AdminProductVariantDto(variant.Id, variant.Sku, variant.Size, variant.Color, variant.WeightInGrams, variant.Capacity, variant.PriceOverride, variant.IsActive, new ProductInventoryDto(inventory.QuantityOnHand, inventory.ReservedQuantity, availableQuantity, inventory.ReorderLevel, availableQuantity <= inventory.ReorderLevel, Convert.ToBase64String(inventory.RowVersion)));
    }

    private static string? EmptyToNull(string value)
    {
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
