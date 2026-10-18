using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Exports.DTOs;
public sealed record ExportQuery(DateTime FromUtc, DateTime ToUtc, LanguageCode Language = LanguageCode.Arabic);
