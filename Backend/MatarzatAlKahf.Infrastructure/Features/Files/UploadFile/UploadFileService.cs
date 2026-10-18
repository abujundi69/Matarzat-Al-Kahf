using System.Security.Cryptography;
using System.Buffers;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Files.Services;
public sealed partial class FileService
{
    public async Task<FileResponse> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Content);

        string originalFileName = GetSafeOriginalFileName(request.FileName);
        string extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        long maximumLength = request.IsPublic
            ? MaximumPublicImageSize
            : MaximumFileSize;
        bool isSupported = ContentTypes.TryGetValue(extension, out string? matchedType);
        bool isPublicImage = !request.IsPublic
            || !string.Equals(matchedType, "application/pdf", StringComparison.Ordinal);
        string suppliedContentType = request.ContentType
            .Split(';', 2, StringSplitOptions.TrimEntries)[0];
        if (request.Length is <= 0
            || request.Length > maximumLength
            || !isSupported
            || !isPublicImage
            || !string.Equals(
                suppliedContentType,
                matchedType,
                StringComparison.OrdinalIgnoreCase))
        {
            string allowed = request.IsPublic
                ? "JPEG, PNG, or WebP image no larger than 5 MB"
                : "JPEG, PNG, WebP, or PDF file no larger than 10 MB";
            throw ValidationException.For("file", $"File must be a valid {allowed}.");
        }

        string expectedType = matchedType!;

        await using var buffer = new MemoryStream(checked((int)request.Length));
        await CopyWithLimitAsync(
            request.Content,
            buffer,
            maximumLength,
            cancellationToken);
        if (buffer.Length != request.Length || !HasValidSignature(buffer, extension))
        {
            throw ValidationException.For("file", "File content does not match its type.");
        }

        string hash = Convert.ToHexString(SHA256.HashData(buffer.GetBuffer().AsSpan(0, checked((int)buffer.Length))));
        string folder = request.IsPublic
            ? GetPublicFolder(request.Purpose)
            : $"private/{DateTime.UtcNow:yyyy/MM}";
        string key = $"{folder}/{Guid.NewGuid():D}{extension}";
        buffer.Position = 0;
        await storage.StoreAsync(key, buffer, cancellationToken);
        var file = new StoredFile
        {
            StorageKey = key,
            OriginalFileName = originalFileName,
            ContentType = expectedType,
            Length = request.Length,
            Sha256 = hash,
            UploadedByAdminUserId = currentActor.AdminUserId,
            UploadedByCustomerId = currentActor.CustomerId,
            IsPublic = request.IsPublic
        };
        dbContext.StoredFiles.Add(file);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await storage.DeleteAsync(key, CancellationToken.None);
            throw;
        }

        await auditService.RecordAsync("file.uploaded", nameof(StoredFile), file.Id, $"File '{file.OriginalFileName}' was uploaded.", cancellationToken);
        return Map(file);
    }

    private static string GetPublicFolder(FileUploadPurpose purpose) => purpose switch
    {
        FileUploadPurpose.Logos => "logos",
        FileUploadPurpose.Products => "products",
        FileUploadPurpose.Categories => "categories",
        FileUploadPurpose.Banners => "banners",
        FileUploadPurpose.Content => "content",
        FileUploadPurpose.Other => "other",
        _ => throw ValidationException.For("purpose", "The upload purpose is invalid.")
    };

    private static string GetSafeOriginalFileName(string fileName)
    {
        string candidate = (fileName ?? string.Empty)
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(candidate) || candidate.Length > 255)
        {
            throw ValidationException.For("file", "The uploaded file name is invalid.");
        }

        return candidate;
    }

    private static async Task CopyWithLimitAsync(
        Stream source,
        Stream destination,
        long maximumLength,
        CancellationToken cancellationToken)
    {
        byte[] copyBuffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            long total = 0;
            while (true)
            {
                int read = await source.ReadAsync(
                    copyBuffer.AsMemory(0, copyBuffer.Length),
                    cancellationToken);
                if (read == 0)
                {
                    break;
                }

                total += read;
                if (total > maximumLength)
                {
                    throw ValidationException.For(
                        "file",
                        $"The uploaded file exceeds the {maximumLength / (1024 * 1024)} MB limit.");
                }

                await destination.WriteAsync(
                    copyBuffer.AsMemory(0, read),
                    cancellationToken);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(copyBuffer);
        }
    }
}
