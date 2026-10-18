using System.Globalization;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Reports.DTOs;
using MatarzatAlKahf.Application.Features.Reports.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Reports.Services;
public sealed partial class ReportService
{
    public async Task<InventoryReportResponse> InventoryAsync(ReportQuery query, CancellationToken cancellationToken)
    {
        Validate(query);
        LanguageCode language = ActiveLanguage;
        IQueryable<Domain.Entities.Catalog.InventoryItem> inventory = dbContext.InventoryItems.Where(item => !item.IsDeleted);
        int count = await inventory.CountAsync(cancellationToken);
        int onHand = await inventory.SumAsync(item => (int? )item.QuantityOnHand, cancellationToken) ?? 0;
        int reserved = await inventory.SumAsync(item => (int? )item.ReservedQuantity, cancellationToken) ?? 0;
        var inventoryRows = await inventory.AsNoTracking()
            .OrderBy(item => item.ProductVariant.Sku)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(item => new
            {
                item.ProductVariantId,
                item.ProductVariant.Sku,
                RequestedName = item.ProductVariant.Product.Translations
                    .Where(translation => translation.Language == language)
                    .Select(translation => translation.Name)
                    .FirstOrDefault(),
                ArabicFallbackName = item.ProductVariant.Product.Translations
                    .Where(translation => translation.Language == LanguageCode.Arabic)
                    .Select(translation => translation.Name)
                    .FirstOrDefault(),
                item.QuantityOnHand,
                item.ReservedQuantity,
                Available = item.QuantityOnHand - item.ReservedQuantity,
                item.ReorderLevel
            })
            .ToListAsync(cancellationToken);
        List<InventoryReportItem> items = inventoryRows.Select(item =>
            new InventoryReportItem(
                item.ProductVariantId,
                item.Sku,
                ResolveLocalizedName(
                    item.RequestedName,
                    item.ArabicFallbackName,
                    item.Sku),
                item.QuantityOnHand,
                item.ReservedQuantity,
                item.Available,
                item.ReorderLevel,
                item.Available > 0 && item.Available <= item.ReorderLevel,
                item.Available <= 0)).ToList();
        return new InventoryReportResponse(onHand, reserved, PagedResultFactory.Create(items, query.Page, query.PageSize, count));
    }
}
