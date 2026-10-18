using MatarzatAlKahf.Application.Features.Exports.DTOs;

namespace MatarzatAlKahf.Application.Features.Exports.Interfaces;
public interface IProductsExportService
{
    Task<ExportFile> ProductsAsync(ExportQuery query, CancellationToken cancellationToken);
}
