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
    private ProductVariant CreateVariant(string sku, string? size, string? color, decimal? weightInGrams, string? capacity, decimal? priceOverride, bool isActive, int initialQuantity, int reorderLevel)
    {
        var variant = new ProductVariant
        {
            Sku = _skuNormalizer.Normalize(sku),
            Size = NormalizeOptional(size),
            Color = NormalizeOptional(color),
            WeightInGrams = weightInGrams,
            Capacity = NormalizeOptional(capacity),
            PriceOverride = priceOverride,
            IsActive = isActive
        };
        var inventory = new InventoryItem
        {
            ProductVariantId = variant.Id,
            QuantityOnHand = initialQuantity,
            ReservedQuantity = 0,
            ReorderLevel = reorderLevel
        };
        variant.InventoryItem = inventory;
        if (initialQuantity > 0)
        {
            inventory.Transactions.Add(new InventoryTransaction { InventoryItemId = inventory.Id, Type = InventoryTransactionType.InitialStock, Quantity = initialQuantity, Notes = "Initial product stock." });
        }

        return variant;
    }
}
