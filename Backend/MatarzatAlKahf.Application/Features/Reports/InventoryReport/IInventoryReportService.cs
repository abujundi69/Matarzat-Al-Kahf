using MatarzatAlKahf.Application.Features.Reports.DTOs;

namespace MatarzatAlKahf.Application.Features.Reports.Interfaces;
public interface IInventoryReportService
{
    Task<InventoryReportResponse> InventoryAsync(ReportQuery query, CancellationToken cancellationToken);
}
