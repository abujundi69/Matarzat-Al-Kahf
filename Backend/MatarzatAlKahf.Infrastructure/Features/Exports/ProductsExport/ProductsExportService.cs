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
    public async Task<ExportFile> ProductsAsync(ExportQuery query, CancellationToken cancellationToken)
    {
        Validate(query);
        var data = await dbContext.Products.AsNoTracking().Where(product => !product.IsDeleted).OrderBy(product => product.Id).Take(MaximumRows).Select(product => new { product.Id, Name = product.Translations.Where(translation => translation.Language == query.Language).Select(translation => translation.Name).FirstOrDefault(), product.BasePrice, product.Currency, product.Status, Sold = product.OrderItems.Where(item => item.Order.Status == OrderStatus.Delivered && item.Order.CreatedAtUtc >= query.FromUtc && item.Order.CreatedAtUtc < query.ToUtc).Sum(item => (int? )item.Quantity) ?? 0, Sales = product.OrderItems.Where(item => item.Order.Status == OrderStatus.Delivered && item.Order.CreatedAtUtc >= query.FromUtc && item.Order.CreatedAtUtc < query.ToUtc).Sum(item => (decimal? )item.TotalPrice) ?? 0 }).ToListAsync(cancellationToken);
        string[] headers = query.Language == LanguageCode.Arabic ? ["المعرف", "المنتج", "السعر", "العملة", "الحالة", "الكمية المباعة", "المبيعات"] : ["Id", "Product", "Price", "Currency", "Status", "Quantity Sold", "Sales"];
        return Build("Products", headers, data.Select(item => new[] { item.Id.ToString(), item.Name ?? item.Id.ToString(), Format(item.BasePrice), item.Currency.ToString(), item.Status.ToString(), Format(item.Sold), Format(item.Sales) }), "products.xlsx");
    }
}
