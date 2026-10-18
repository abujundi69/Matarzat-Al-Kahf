using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Application.Features.Exports.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Exports.Services;
public sealed partial class ExportService
{
    public async Task<ExportFile> InventoryAsync(ExportQuery query, CancellationToken cancellationToken)
    {
        Validate(query);
        var data = await dbContext.InventoryItems.AsNoTracking().Where(item => !item.IsDeleted).OrderBy(item => item.ProductVariant.Sku).Take(MaximumRows).Select(item => new { item.ProductVariant.Sku, Name = item.ProductVariant.Product.Translations.Where(translation => translation.Language == query.Language).Select(translation => translation.Name).FirstOrDefault(), item.QuantityOnHand, item.ReservedQuantity, Available = item.QuantityOnHand - item.ReservedQuantity, item.ReorderLevel }).ToListAsync(cancellationToken);
        string[] headers = query.Language == LanguageCode.Arabic ? ["SKU", "المنتج", "المخزون", "المحجوز", "المتاح", "حد إعادة الطلب"] : ["SKU", "Product", "On Hand", "Reserved", "Available", "Reorder Level"];
        return Build("Inventory", headers, data.Select(item => new[] { item.Sku, item.Name ?? item.Sku, Format(item.QuantityOnHand), Format(item.ReservedQuantity), Format(item.Available), Format(item.ReorderLevel) }), "inventory.xlsx");
    }
}
