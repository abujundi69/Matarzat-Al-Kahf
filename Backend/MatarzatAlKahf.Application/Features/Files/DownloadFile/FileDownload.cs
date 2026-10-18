namespace MatarzatAlKahf.Application.Features.Files.DTOs.Responses;

public sealed record FileDownload(
    Stream Content,
    string ContentType,
    string FileName);
