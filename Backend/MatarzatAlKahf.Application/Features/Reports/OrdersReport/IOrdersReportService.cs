using MatarzatAlKahf.Application.Features.Reports.DTOs;

namespace MatarzatAlKahf.Application.Features.Reports.Interfaces;
public interface IOrdersReportService
{
    Task<OrderReportResponse> OrdersAsync(ReportQuery query, CancellationToken cancellationToken);
}
