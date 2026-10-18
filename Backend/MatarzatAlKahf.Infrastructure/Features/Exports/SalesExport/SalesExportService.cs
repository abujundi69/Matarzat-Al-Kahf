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
    public async Task<ExportFile> SalesAsync(ExportQuery query, CancellationToken cancellationToken)
    {
        Validate(query);
        var data = await dbContext.Orders.AsNoTracking().Where(order => !order.IsDeleted && order.Status == OrderStatus.Delivered && order.CreatedAtUtc >= query.FromUtc && order.CreatedAtUtc < query.ToUtc).GroupBy(order => order.CreatedAtUtc.Date).Select(group => new { Date = group.Key, Orders = group.Count(), Sales = group.Sum(order => order.TotalAmount) }).OrderBy(item => item.Date).ToListAsync(cancellationToken);
        string[] headers = query.Language == LanguageCode.Arabic ? ["التاريخ", "عدد الطلبات", "المبيعات"] : ["Date", "Order Count", "Sales"];
        return Build("Sales", headers, data.Select(item => new[] { item.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Format(item.Orders), Format(item.Sales) }), "sales.xlsx");
    }
}
