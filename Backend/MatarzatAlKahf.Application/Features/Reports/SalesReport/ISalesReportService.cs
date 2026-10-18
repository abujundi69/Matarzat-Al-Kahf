using MatarzatAlKahf.Application.Features.Reports.DTOs;

namespace MatarzatAlKahf.Application.Features.Reports.Interfaces;
public interface ISalesReportService
{
    Task<SalesReportResponse> SalesAsync(ReportQuery query, CancellationToken cancellationToken);
}
