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
    private void UpdateVariants(Product product, IReadOnlyList<UpdateProductVariantRequest> inputs)
    {
        var submittedIds = inputs.Where(input => input.Id.HasValue).Select(input => input.Id!.Value).ToHashSet();
        foreach (ProductVariant existing in product.Variants.Where(variant => !variant.IsDeleted))
        {
            if (!submittedIds.Contains(existing.Id))
            {
                existing.IsActive = false;
                existing.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        foreach (UpdateProductVariantRequest input in inputs)
        {
            if (!input.Id.HasValue)
            {
                product.Variants.Add(CreateVariant(input.Sku, input.Size, input.Color, input.WeightInGrams, input.Capacity, input.PriceOverride, input.IsActive, input.InitialQuantity ?? 0, input.ReorderLevel));
                continue;
            }

            ProductVariant variant = product.Variants.SingleOrDefault(entity => entity.Id == input.Id.Value && !entity.IsDeleted) ?? throw new NotFoundException(nameof(ProductVariant), input.Id.Value);
            variant.Sku = _skuNormalizer.Normalize(input.Sku);
            variant.Size = NormalizeOptional(input.Size);
            variant.Color = NormalizeOptional(input.Color);
            variant.WeightInGrams = input.WeightInGrams;
            variant.Capacity = NormalizeOptional(input.Capacity);
            variant.PriceOverride = input.PriceOverride;
            variant.IsActive = input.IsActive;
            variant.UpdatedAtUtc = DateTime.UtcNow;
            variant.InventoryItem.ReorderLevel = input.ReorderLevel;
            variant.InventoryItem.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
