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
    public async Task<OrderReportResponse> OrdersAsync(ReportQuery query, CancellationToken cancellationToken)
    {
        Validate(query);
        var orders = dbContext.Orders.AsNoTracking().Where(order => !order.IsDeleted && order.CreatedAtUtc >= query.FromUtc && order.CreatedAtUtc < query.ToUtc);
        int newCount = await orders.CountAsync(order => order.Status == OrderStatus.New, cancellationToken);
        int complete = await orders.CountAsync(order => order.Status == OrderStatus.Delivered, cancellationToken);
        int cancelled = await orders.CountAsync(order => order.Status == OrderStatus.Cancelled, cancellationToken);
        int returned = await orders.CountAsync(order => order.Status == OrderStatus.Returned, cancellationToken);
        var cityRows = await orders.GroupBy(order => order.City).Select(group => new { Name = group.Key, Count = group.Count(), Amount = group.Sum(order => order.TotalAmount) }).OrderByDescending(item => item.Count).ToListAsync(cancellationToken);
        List<NamedMetricResponse> cities = cityRows.Select(item => new NamedMetricResponse(item.Name, item.Count, item.Amount)).ToList();
        var payment = await orders.GroupBy(order => order.SelectedPaymentMethod).Select(group => new { Method = group.Key, Count = group.Count(), Amount = group.Sum(order => order.TotalAmount) }).ToListAsync(cancellationToken);
        return new OrderReportResponse(newCount, complete, cancelled, returned, cities, payment.Select(item => new NamedMetricResponse(item.Method.ToString(), item.Count, item.Amount)).ToArray());
    }
}
