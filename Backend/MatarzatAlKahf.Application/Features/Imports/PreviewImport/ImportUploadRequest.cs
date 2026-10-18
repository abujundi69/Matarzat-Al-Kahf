using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.DataImports.DTOs;
public sealed record ImportUploadRequest(string FileName, long Length, Stream Content, bool AllowNewProducts = false);
