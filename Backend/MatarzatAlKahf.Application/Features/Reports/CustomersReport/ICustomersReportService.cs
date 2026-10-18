using MatarzatAlKahf.Application.Features.Reports.DTOs;

namespace MatarzatAlKahf.Application.Features.Reports.Interfaces;
public interface ICustomersReportService
{
    Task<CustomerReportResponse> CustomersAsync(ReportQuery query, CancellationToken cancellationToken);
}
