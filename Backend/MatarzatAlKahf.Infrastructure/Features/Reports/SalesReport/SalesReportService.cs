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
    public async Task<SalesReportResponse> SalesAsync(ReportQuery query, CancellationToken cancellationToken)
    {
        Validate(query);
        LanguageCode language = ActiveLanguage;
        var orders = dbContext.Orders.AsNoTracking().Where(order => !order.IsDeleted && order.CreatedAtUtc >= query.FromUtc && order.CreatedAtUtc < query.ToUtc);
        int orderCount = await orders.CountAsync(cancellationToken);
        var deliveredOrders = orders.Where(order => order.Status == OrderStatus.Delivered);
        int deliveredOrderCount = await deliveredOrders.CountAsync(cancellationToken);
        decimal total = await deliveredOrders.SumAsync(order => (decimal? )order.TotalAmount, cancellationToken) ?? 0;
        var dailyRows = await deliveredOrders.GroupBy(order => order.CreatedAtUtc.Date).Select(group => new { DateUtc = group.Key, OrderCount = group.Count(), Sales = group.Sum(order => order.TotalAmount) }).OrderBy(item => item.DateUtc).ToListAsync(cancellationToken);
        List<DateSalesResponse> daily = dailyRows.Select(item => new DateSalesResponse(item.DateUtc, item.OrderCount, item.Sales)).ToList();
        var paymentGroups = await orders.GroupBy(order => order.SelectedPaymentMethod).Select(group => new { Name = group.Key, Count = group.Count(), Amount = group.Sum(order => order.TotalAmount) }).ToListAsync(cancellationToken);
        var statusGroups = await orders.GroupBy(order => order.Status).Select(group => new { Name = group.Key, Count = group.Count(), Amount = group.Sum(order => order.TotalAmount) }).ToListAsync(cancellationToken);
        var categoryRows = await dbContext.OrderItems.AsNoTracking()
            .Where(item =>
                !item.IsDeleted
                && !item.Order.IsDeleted
                && item.Order.CreatedAtUtc >= query.FromUtc
                && item.Order.CreatedAtUtc < query.ToUtc
                && item.Order.Status == OrderStatus.Delivered)
            .GroupBy(item => new
            {
                item.Product.CategoryId,
                RequestedName = item.Product.Category.Translations
                    .Where(translation => translation.Language == language)
                    .Select(translation => translation.Name)
                    .FirstOrDefault(),
                ArabicFallbackName = item.Product.Category.Translations
                    .Where(translation => translation.Language == LanguageCode.Arabic)
                    .Select(translation => translation.Name)
                    .FirstOrDefault()
            })
            .Select(group => new
            {
                group.Key.RequestedName,
                group.Key.ArabicFallbackName,
                group.Key.CategoryId,
                Count = group.Sum(item => item.Quantity),
                Amount = group.Sum(item => item.TotalPrice)
            })
            .OrderByDescending(item => item.Amount)
            .ToListAsync(cancellationToken);
        List<NamedMetricResponse> categories = categoryRows.Select(item =>
            new NamedMetricResponse(
                ResolveLocalizedName(
                    item.RequestedName,
                    item.ArabicFallbackName,
                    item.CategoryId.ToString()),
                item.Count,
                item.Amount)).ToList();
        PeriodSalesResponse[] weekly = daily.GroupBy(item => StartOfWeek(item.DateUtc)).Select(group => new PeriodSalesResponse(group.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), group.Sum(item => item.OrderCount), group.Sum(item => item.Sales))).ToArray();
        PeriodSalesResponse[] monthly = daily.GroupBy(item => new { item.DateUtc.Year, item.DateUtc.Month }).Select(group => new PeriodSalesResponse(string.Create(CultureInfo.InvariantCulture, $"{group.Key.Year:D4}-{group.Key.Month:D2}"), group.Sum(item => item.OrderCount), group.Sum(item => item.Sales))).ToArray();
        return new SalesReportResponse(total, orderCount, deliveredOrderCount == 0 ? 0 : decimal.Round(total / deliveredOrderCount, 2), daily, weekly, monthly, paymentGroups.Select(item => new NamedMetricResponse(item.Name.ToString(), item.Count, item.Amount)).ToArray(), statusGroups.Select(item => new NamedMetricResponse(item.Name.ToString(), item.Count, item.Amount)).ToArray(), categories);
    }
}
