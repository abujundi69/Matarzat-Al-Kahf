namespace MatarzatAlKahf.Application.Features.Files.DTOs.Requests;

public sealed record ProductImageUploadRequest(
    FileUploadRequest File,
    string? ArabicAltText,
    string? EnglishAltText,
    int SortOrder,
    bool IsPrimary);
