using MatarzatAlKahf.Application.Features.Reports.DTOs;

namespace MatarzatAlKahf.Application.Features.Reports.Interfaces;
public interface IProductsReportService
{
    Task<ProductReportResponse> ProductsAsync(ReportQuery query, CancellationToken cancellationToken);
}
