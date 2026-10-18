using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.DataImports.DTOs;
public sealed record ImportCommitResponse(Guid ImportId, ImportType Type, int Inserted, int Updated, bool IsDuplicate, DateTime CompletedAtUtc);
