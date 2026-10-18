using MatarzatAlKahf.Application.Features.Dashboard.DTOs;
using MatarzatAlKahf.Application.Features.Dashboard.Interfaces;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Dashboard.Services;

public sealed class DashboardService(
    MatarzatAlKahfDbContext dbContext) : IDashboardService
{
    public async Task<DashboardResponse> GetAsync(CancellationToken cancellationToken)
    {
        DateTime today = DateTime.UtcNow.Date;
        DateTime tomorrow = today.AddDays(1);
        DateTime month = new(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        IQueryable<Domain.Entities.Orders.Order> delivered =
            dbContext.Orders.Where(order =>
                !order.IsDeleted && order.Status == OrderStatus.Delivered);
        int newOrders = await dbContext.Orders.CountAsync(
            order => !order.IsDeleted && order.Status == OrderStatus.New,
            cancellationToken);
        decimal todaySales = await delivered
            .Where(order => order.CreatedAtUtc >= today && order.CreatedAtUtc < tomorrow)
            .SumAsync(order => (decimal?)order.TotalAmount, cancellationToken) ?? 0;
        decimal monthSales = await delivered
            .Where(order => order.CreatedAtUtc >= month)
            .SumAsync(order => (decimal?)order.TotalAmount, cancellationToken) ?? 0;
        int customers = await dbContext.Customers.CountAsync(
            customer => !customer.IsDeleted,
            cancellationToken);
        int lowStock = await dbContext.InventoryItems.CountAsync(
            inventory => !inventory.IsDeleted
                && inventory.QuantityOnHand - inventory.ReservedQuantity > 0
                && inventory.QuantityOnHand - inventory.ReservedQuantity <= inventory.ReorderLevel,
            cancellationToken);
        int outOfStock = await dbContext.InventoryItems.CountAsync(
            inventory => !inventory.IsDeleted
                && inventory.QuantityOnHand - inventory.ReservedQuantity <= 0,
            cancellationToken);
        int pendingPayments = await dbContext.Payments.CountAsync(
            payment => !payment.IsDeleted && payment.Status == PaymentStatus.Processing,
            cancellationToken);
        var bestRows = await dbContext.OrderItems.AsNoTracking()
            .Where(item => !item.IsDeleted && item.Order.Status == OrderStatus.Delivered)
            .GroupBy(item => new
            {
                item.ProductId,
                item.SkuSnapshot,
                item.ProductNameSnapshot
            })
            .Select(group => new
            {
                group.Key.ProductId,
                group.Key.SkuSnapshot,
                group.Key.ProductNameSnapshot,
                QuantitySold = group.Sum(item => item.Quantity),
                Revenue = group.Sum(item => item.TotalPrice)
            })
            .OrderByDescending(item => item.QuantitySold)
            .Take(10)
            .ToListAsync(cancellationToken);
        List<DashboardProductResponse> best = bestRows.Select(item =>
            new DashboardProductResponse(
                item.ProductId,
                item.SkuSnapshot,
                item.ProductNameSnapshot,
                item.QuantitySold,
                item.Revenue)).ToList();
        List<OrderSummaryResponse> latest = await dbContext.Orders.AsNoTracking()
            .Where(order => !order.IsDeleted)
            .OrderByDescending(order => order.CreatedAtUtc)
            .Take(10)
            .Select(order => new OrderSummaryResponse(
                order.Id,
                order.OrderNumber,
                order.CustomerName,
                order.PhoneNumber,
                order.Status,
                order.SelectedPaymentMethod,
                order.Payments.OrderByDescending(payment => payment.CreatedAtUtc)
                    .Select(payment => payment.Status).FirstOrDefault(),
                order.TotalAmount,
                order.Currency,
                order.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        List<DashboardNotificationResponse> notifications =
            await dbContext.Notifications.AsNoTracking()
                .Where(item => !item.IsDeleted
                    && (item.Status == NotificationStatus.Failed
                        || item.Status == NotificationStatus.Pending))
                .OrderByDescending(item => item.CreatedAtUtc)
                .Take(10)
                .Select(item => new DashboardNotificationResponse(
                    item.Id,
                    item.Channel,
                    item.Status,
                    item.Title,
                    item.CreatedAtUtc))
                .ToListAsync(cancellationToken);
        return new DashboardResponse(
            newOrders,
            todaySales,
            monthSales,
            customers,
            lowStock,
            outOfStock,
            pendingPayments,
            best,
            latest,
            notifications);
    }
}
