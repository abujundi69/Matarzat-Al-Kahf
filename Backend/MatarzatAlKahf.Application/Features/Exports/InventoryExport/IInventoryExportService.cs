using MatarzatAlKahf.Application.Features.Exports.DTOs;

namespace MatarzatAlKahf.Application.Features.Exports.Interfaces;
public interface IInventoryExportService
{
    Task<ExportFile> InventoryAsync(ExportQuery query, CancellationToken cancellationToken);
}
