using MatarzatAlKahf.Application.Features.DataImports.DTOs;
using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.DataImports.Interfaces;
public interface ICommitImportService
{
    Task<ImportCommitResponse> CommitAsync(Guid importId, ImportType type, CommitImportRequest request, string idempotencyKey, CancellationToken cancellationToken);
}
