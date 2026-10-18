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
    public async Task<ExportFile> OrdersAsync(ExportQuery query, CancellationToken cancellationToken)
    {
        Validate(query);
        var data = await dbContext.Orders.AsNoTracking().Where(order => !order.IsDeleted && order.CreatedAtUtc >= query.FromUtc && order.CreatedAtUtc < query.ToUtc).OrderByDescending(order => order.CreatedAtUtc).Take(MaximumRows).Select(order => new { order.OrderNumber, order.CustomerName, order.PhoneNumber, order.City, order.Status, order.SelectedPaymentMethod, order.TotalAmount, order.Currency, order.CreatedAtUtc }).ToListAsync(cancellationToken);
        string[] headers = query.Language == LanguageCode.Arabic ? ["رقم الطلب", "العميل", "الهاتف", "المدينة", "الحالة", "طريقة الدفع", "الإجمالي", "العملة", "التاريخ"] : ["Order Number", "Customer", "Phone", "City", "Status", "Payment Method", "Total", "Currency", "Date"];
        return Build("Orders", headers, data.Select(item => new[] { item.OrderNumber, item.CustomerName, item.PhoneNumber, item.City, item.Status.ToString(), item.SelectedPaymentMethod.ToString(), Format(item.TotalAmount), item.Currency.ToString(), Format(item.CreatedAtUtc) }), "orders.xlsx");
    }
}
