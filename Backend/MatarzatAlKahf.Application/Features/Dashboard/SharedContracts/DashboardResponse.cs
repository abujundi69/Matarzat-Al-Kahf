using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Dashboard.DTOs;
public sealed record DashboardResponse(int NewOrderCount, decimal TodaySales, decimal CurrentMonthSales, int CustomerCount, int LowStockProductCount, int OutOfStockProductCount, int PendingPaymentVerificationCount, IReadOnlyList<DashboardProductResponse> BestSellingProducts, IReadOnlyList<OrderSummaryResponse> LatestOrders, IReadOnlyList<DashboardNotificationResponse> ImportantNotifications);
