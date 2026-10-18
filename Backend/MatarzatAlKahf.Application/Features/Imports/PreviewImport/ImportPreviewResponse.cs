using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.DataImports.DTOs;
public sealed record ImportPreviewResponse(Guid ImportId, ImportType Type, int TotalRows, int ValidRows, int InvalidRows, IReadOnlyList<ImportRowError> Errors, DateTime ExpiresAtUtc);
