using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Reports.DTOs;
public sealed record SalesReportResponse(decimal TotalSales, int OrderCount, decimal AverageOrderValue, IReadOnlyList<DateSalesResponse> Daily, IReadOnlyList<PeriodSalesResponse> Weekly, IReadOnlyList<PeriodSalesResponse> Monthly, IReadOnlyList<NamedMetricResponse> ByPaymentMethod, IReadOnlyList<NamedMetricResponse> ByOrderStatus, IReadOnlyList<NamedMetricResponse> ByCategory);
