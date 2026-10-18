namespace MatarzatAlKahf.Application.Features.Files.Interfaces;

public interface IUploadedFileCleanupService
{
    Task DeleteIfUnreferencedAsync(
        string? fileUrl,
        CancellationToken cancellationToken);
}
