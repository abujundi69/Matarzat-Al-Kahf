using MatarzatAlKahf.Application.Features.Reports.DTOs;

namespace MatarzatAlKahf.Application.Features.Reports.Interfaces;
public interface IReportService : ISalesReportService, IProductsReportService, ICustomersReportService, IOrdersReportService, IInventoryReportService
{
}
