using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.DataImports.DTOs;
public sealed record ImportRowError(int RowNumber, string Field, string Message);
