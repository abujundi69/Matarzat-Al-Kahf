using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Application.Inventory;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Inventory;
public sealed partial class AdminInventoryService
{
    private IQueryable<InventoryReadRow> InventoryRows(Guid? productVariantId = null, bool lowStockOnly = false)
    {
        IQueryable<InventoryItem> query = _dbContext.InventoryItems.AsNoTracking().Where(inventory => !inventory.IsDeleted && !inventory.ProductVariant.IsDeleted && !inventory.ProductVariant.Product.IsDeleted);
        if (productVariantId.HasValue)
        {
            query = query.Where(inventory => inventory.ProductVariantId == productVariantId.Value);
        }

        if (lowStockOnly)
        {
            query = query.Where(inventory => inventory.QuantityOnHand - inventory.ReservedQuantity <= inventory.ReorderLevel);
        }

        return query.OrderBy(inventory => inventory.ProductVariant.Sku).ThenBy(inventory => inventory.ProductVariantId).Select(inventory => new InventoryReadRow(inventory.ProductVariantId, inventory.ProductVariant.ProductId, inventory.ProductVariant.Sku, inventory.ProductVariant.Size, inventory.ProductVariant.Color, inventory.QuantityOnHand, inventory.ReservedQuantity, inventory.ReorderLevel, inventory.RowVersion));
    }
}
