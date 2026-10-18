using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.DataImports.DTOs;
using MatarzatAlKahf.Application.Features.DataImports.Interfaces;
using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.DataImports.Services;
public sealed partial class ImportService
{
    public async Task<ImportPreviewResponse> PreviewAsync(ImportType type, ImportUploadRequest request, CancellationToken cancellationToken)
    {
        Guid adminId = currentActor.AdminUserId ?? throw new AuthenticationFailedException("Administrator authentication is required.");
        ValidateFile(request);
        await using var content = new MemoryStream(checked((int)request.Length));
        await request.Content.CopyToAsync(content, cancellationToken);
        if (content.Length != request.Length)
        {
            throw ValidationException.For(nameof(request), "Uploaded file length is invalid.");
        }

        string hash = Convert.ToHexString(SHA256.HashData(content.GetBuffer().AsSpan(0, checked((int)content.Length))));
        content.Position = 0;
        ParsedRows parsed = type == ImportType.Products ? await ParseProductsAsync(content, request.AllowNewProducts, cancellationToken) : await ParseInventoryAsync(content, cancellationToken);
        var operation = new ImportOperation
        {
            Type = type,
            Status = ImportStatus.Previewed,
            AdminUserId = adminId,
            OriginalFileName = Path.GetFileName(request.FileName),
            FileHash = hash,
            RowsJson = parsed.RowsJson,
            ErrorsJson = JsonSerializer.Serialize(parsed.Errors),
            TotalRows = parsed.TotalRows,
            ValidRows = parsed.TotalRows - parsed.Errors.Select(error => error.RowNumber).Distinct().Count(),
            CommitIdempotencyKey = string.Empty,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(2)
        };
        dbContext.ImportOperations.Add(operation);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.RecordAsync("import.previewed", nameof(ImportOperation), operation.Id, $"{type} import previewed with {operation.TotalRows} rows.", cancellationToken);
        return MapPreview(operation, parsed.Errors);
    }
}
