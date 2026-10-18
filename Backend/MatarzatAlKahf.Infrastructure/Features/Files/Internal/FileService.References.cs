using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Files.Services;

public sealed partial class FileService
{
    public async Task DeleteIfUnreferencedAsync(
        string? fileUrl,
        CancellationToken cancellationToken)
    {
        StoredFile? file = await FindTrackedFileAsync(fileUrl, cancellationToken);
        if (file is null || await IsReferencedAsync(file, cancellationToken))
        {
            return;
        }

        await DeleteTrackedFileAsync(file, cancellationToken);
    }

    private async Task<StoredFile?> FindTrackedFileAsync(
        string? fileUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)
            || Uri.TryCreate(fileUrl, UriKind.Absolute, out _)
            || fileUrl.Contains('?')
            || fileUrl.Contains('#'))
        {
            return null;
        }

        const string downloadPrefix = "/api/v1/files/";
        if (fileUrl.StartsWith(downloadPrefix, StringComparison.OrdinalIgnoreCase))
        {
            string idText = fileUrl[downloadPrefix.Length..];
            if (!Guid.TryParseExact(idText, "D", out Guid id)
                && !Guid.TryParseExact(idText, "N", out id))
            {
                return null;
            }

            return await dbContext.StoredFiles.SingleOrDefaultAsync(
                item => item.Id == id && !item.IsDeleted,
                cancellationToken);
        }

        const string uploadPrefix = "/uploads/";
        if (!fileUrl.StartsWith(uploadPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string key = fileUrl[uploadPrefix.Length..].Replace('\\', '/');
        string[] segments = key.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 2
            || !FileStorageOptions.PublicFolders.Contains(
                segments[0],
                StringComparer.OrdinalIgnoreCase)
            || !Guid.TryParseExact(
                Path.GetFileNameWithoutExtension(segments[1]),
                "D",
                out _)
            || !ContentTypes.ContainsKey(Path.GetExtension(segments[1])))
        {
            return null;
        }

        string normalizedKey = $"{segments[0].ToLowerInvariant()}/{segments[1].ToLowerInvariant()}";
        return await dbContext.StoredFiles.SingleOrDefaultAsync(
            item => item.StorageKey == normalizedKey && !item.IsDeleted,
            cancellationToken);
    }

    private async Task<bool> IsReferencedAsync(
        StoredFile file,
        CancellationToken cancellationToken)
    {
        string apiUrl = $"/api/v1/files/{file.Id}";
        string staticUrl = $"/uploads/{file.StorageKey.Replace('\\', '/')}";
        string[] urls = [apiUrl, staticUrl];

        if (await dbContext.Categories.AsNoTracking().AnyAsync(
                item => !item.IsDeleted && urls.Contains(item.ImageUrl),
                cancellationToken)
            || await dbContext.ProductImages.AsNoTracking().AnyAsync(
                item => !item.IsDeleted
                    && !item.Product.IsDeleted
                    && (urls.Contains(item.ImageUrl)
                        || urls.Contains(item.ThumbnailUrl)),
                cancellationToken)
            || await dbContext.BannerTranslations.AsNoTracking().AnyAsync(
                item => !item.IsDeleted
                    && !item.Banner.IsDeleted
                    && (urls.Contains(item.DesktopImageUrl)
                        || urls.Contains(item.MobileImageUrl)),
                cancellationToken)
            || await dbContext.StoreSettings.AsNoTracking().AnyAsync(
                item => !item.IsDeleted
                    && item.Value != null
                    && urls.Contains(item.Value),
                cancellationToken)
            || await dbContext.StoreSettingTranslations.AsNoTracking().AnyAsync(
                item => !item.IsDeleted
                    && !item.StoreSetting.IsDeleted
                    && urls.Contains(item.Value),
                cancellationToken)
            || await dbContext.SeoSettings.AsNoTracking().AnyAsync(
                item => !item.IsDeleted
                    && (urls.Contains(item.ArabicOpenGraphImageUrl)
                        || urls.Contains(item.EnglishOpenGraphImageUrl)),
                cancellationToken)
            || await dbContext.Payments.AsNoTracking().AnyAsync(
                item => !item.IsDeleted
                    && !item.Order.IsDeleted
                    && urls.Contains(item.ProofFilePath),
                cancellationToken))
        {
            return true;
        }

        foreach (string url in urls)
        {
            bool contentReference = await dbContext.ContentPageTranslations
                .AsNoTracking()
                .AnyAsync(
                    item => !item.IsDeleted
                        && !item.ContentPage.IsDeleted
                        && item.Content.Contains(url),
                    cancellationToken);
            if (contentReference)
            {
                return true;
            }
        }

        return false;
    }

    private async Task DeleteTrackedFileAsync(
        StoredFile file,
        CancellationToken cancellationToken)
    {
        file.IsDeleted = true;
        file.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await storage.DeleteAsync(file.StorageKey, cancellationToken);
        await auditService.RecordAsync(
            "file.deleted",
            nameof(StoredFile),
            file.Id,
            $"File '{file.OriginalFileName}' was deleted.",
            cancellationToken);
    }
}
