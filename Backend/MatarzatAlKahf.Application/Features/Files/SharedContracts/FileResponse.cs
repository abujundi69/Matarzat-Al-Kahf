namespace MatarzatAlKahf.Application.Features.Files.DTOs.Responses;

public sealed record FileResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long Length,
    string Url,
    DateTime CreatedAtUtc);
