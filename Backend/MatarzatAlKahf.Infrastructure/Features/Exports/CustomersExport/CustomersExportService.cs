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
    public async Task<ExportFile> CustomersAsync(ExportQuery query, CancellationToken cancellationToken)
    {
        Validate(query);
        var data = await dbContext.Customers.AsNoTracking().Where(customer => !customer.IsDeleted).OrderByDescending(customer => customer.CreatedAtUtc).Take(MaximumRows).Select(customer => new { customer.FullName, customer.PhoneNumber, customer.Email, customer.Status, Orders = customer.Orders.Count(order => order.CreatedAtUtc >= query.FromUtc && order.CreatedAtUtc < query.ToUtc), Spending = customer.Orders.Where(order => order.Status == OrderStatus.Delivered && order.CreatedAtUtc >= query.FromUtc && order.CreatedAtUtc < query.ToUtc).Sum(order => (decimal? )order.TotalAmount) ?? 0, customer.CreatedAtUtc }).ToListAsync(cancellationToken);
        string[] headers = query.Language == LanguageCode.Arabic ? ["العميل", "الهاتف", "البريد", "الحالة", "الطلبات", "الإنفاق", "تاريخ التسجيل"] : ["Customer", "Phone", "Email", "Status", "Orders", "Spending", "Registered"];
        return Build("Customers", headers, data.Select(item => new[] { item.FullName, item.PhoneNumber, item.Email, item.Status.ToString(), Format(item.Orders), Format(item.Spending), Format(item.CreatedAtUtc) }), "customers.xlsx");
    }
}
