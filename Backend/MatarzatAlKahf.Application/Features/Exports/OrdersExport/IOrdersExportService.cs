using MatarzatAlKahf.Application.Features.Exports.DTOs;

namespace MatarzatAlKahf.Application.Features.Exports.Interfaces;
public interface IOrdersExportService
{
    Task<ExportFile> OrdersAsync(ExportQuery query, CancellationToken cancellationToken);
}
