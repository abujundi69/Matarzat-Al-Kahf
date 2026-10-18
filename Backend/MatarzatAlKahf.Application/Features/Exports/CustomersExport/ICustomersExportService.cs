using MatarzatAlKahf.Application.Features.Exports.DTOs;

namespace MatarzatAlKahf.Application.Features.Exports.Interfaces;
public interface ICustomersExportService
{
    Task<ExportFile> CustomersAsync(ExportQuery query, CancellationToken cancellationToken);
}
