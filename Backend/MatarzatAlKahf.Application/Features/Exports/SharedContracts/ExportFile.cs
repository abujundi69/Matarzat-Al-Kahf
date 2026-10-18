using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Exports.DTOs;
public sealed record ExportFile(Stream Content, string ContentType, string FileName);
