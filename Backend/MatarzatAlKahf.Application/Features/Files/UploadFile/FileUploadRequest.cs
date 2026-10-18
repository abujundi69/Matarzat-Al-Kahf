namespace MatarzatAlKahf.Application.Features.Files.DTOs.Requests;

public sealed record FileUploadRequest(
    string FileName,
    string ContentType,
    long Length,
    Stream Content,
    bool IsPublic = false,
    FileUploadPurpose Purpose = FileUploadPurpose.Other);
