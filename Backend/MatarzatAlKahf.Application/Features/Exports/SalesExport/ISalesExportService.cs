using MatarzatAlKahf.Application.Features.Exports.DTOs;

namespace MatarzatAlKahf.Application.Features.Exports.Interfaces;
public interface ISalesExportService
{
    Task<ExportFile> SalesAsync(ExportQuery query, CancellationToken cancellationToken);
}
