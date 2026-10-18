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
    public async Task<ProductReportResponse> ProductsAsync(ReportQuery query, CancellationToken cancellationToken)
    {
        Validate(query);
        LanguageCode language = ActiveLanguage;
        IQueryable<Domain.Entities.Catalog.Product> products = dbContext.Products.Where(product => !product.IsDeleted);
        int count = await products.CountAsync(cancellationToken);
        var productRows = await products.AsNoTracking().Select(product => new
        {
            product.Id,
            RequestedName = product.Translations
                .Where(translation => translation.Language == language)
                .Select(translation => translation.Name)
                .FirstOrDefault(),
            ArabicFallbackName = product.Translations
                .Where(translation => translation.Language == LanguageCode.Arabic)
                .Select(translation => translation.Name)
                .FirstOrDefault(),
            Sku = product.Variants
                .OrderBy(variant => variant.CreatedAtUtc)
                .Select(variant => variant.Sku)
                .FirstOrDefault() ?? string.Empty,
            QuantitySold = product.OrderItems
                .Where(item =>
                    item.Order.CreatedAtUtc >= query.FromUtc
                    && item.Order.CreatedAtUtc < query.ToUtc
                    && item.Order.Status == OrderStatus.Delivered)
                .Sum(item => (int?)item.Quantity) ?? 0,
            Revenue = product.OrderItems
                .Where(item =>
                    item.Order.CreatedAtUtc >= query.FromUtc
                    && item.Order.CreatedAtUtc < query.ToUtc
                    && item.Order.Status == OrderStatus.Delivered)
                .Sum(item => (decimal?)item.TotalPrice) ?? 0,
            Available = product.Variants
                .Where(variant => !variant.IsDeleted)
                .Sum(variant =>
                    (int?)(variant.InventoryItem.QuantityOnHand
                        - variant.InventoryItem.ReservedQuantity)) ?? 0
        }).OrderByDescending(item => item.QuantitySold)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        List<ProductReportItem> items = productRows.Select(item =>
            new ProductReportItem(
                item.Id,
                ResolveLocalizedName(
                    item.RequestedName,
                    item.ArabicFallbackName,
                    item.Id.ToString()),
                item.Sku,
                item.QuantitySold,
                item.Revenue,
                item.Available)).ToList();
        int low = await dbContext.InventoryItems.CountAsync(item => !item.IsDeleted && item.QuantityOnHand - item.ReservedQuantity > 0 && item.QuantityOnHand - item.ReservedQuantity <= item.ReorderLevel, cancellationToken);
        int none = await dbContext.InventoryItems.CountAsync(item => !item.IsDeleted && item.QuantityOnHand - item.ReservedQuantity <= 0, cancellationToken);
        return new ProductReportResponse(PagedResultFactory.Create(items, query.Page, query.PageSize, count), low, none);
    }
}
