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
    private static AdminInventoryDto Map(InventoryReadRow row, string productName)
    {
        int available = row.QuantityOnHand - row.ReservedQuantity;
        return new AdminInventoryDto(row.ProductVariantId, row.ProductId, productName, row.Sku, row.Size, row.Color, row.QuantityOnHand, row.ReservedQuantity, available, row.ReorderLevel, available <= row.ReorderLevel, Convert.ToBase64String(row.RowVersion));
    }
}
