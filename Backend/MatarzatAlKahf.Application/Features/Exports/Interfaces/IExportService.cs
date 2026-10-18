using MatarzatAlKahf.Application.Features.Exports.DTOs;

namespace MatarzatAlKahf.Application.Features.Exports.Interfaces;
public interface IExportService : IOrdersExportService, ISalesExportService, IProductsExportService, ICustomersExportService, IInventoryExportService
{
}
