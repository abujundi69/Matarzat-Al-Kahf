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
    private async Task<ImportCommitResponse> CommitCoreAsync(Guid importId, ImportType type, Guid adminId, string idempotencyKey, CancellationToken cancellationToken)
    {
        ImportOperation operation = await dbContext.ImportOperations.SingleOrDefaultAsync(item => item.Id == importId && item.Type == type && item.AdminUserId == adminId && !item.IsDeleted, cancellationToken) ?? throw new NotFoundException("Import preview was not found.");
        if (operation.Status == ImportStatus.Completed)
        {
            if (operation.CommitIdempotencyKey != idempotencyKey)
            {
                throw new ConflictException("Import was already committed with another key.");
            }

            return new ImportCommitResponse(operation.Id, operation.Type, operation.InsertedRows, operation.UpdatedRows, true, operation.CompletedAtUtc!.Value);
        }

        if (operation.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new ConflictException("Import preview has expired.");
        }

        ImportRowError[] errors = JsonSerializer.Deserialize<ImportRowError[]>(operation.ErrorsJson) ?? [];
        if (errors.Length > 0)
        {
            throw new ConflictException("Import preview contains validation errors.");
        }

        operation.Status = ImportStatus.Committing;
        operation.CommitIdempotencyKey = idempotencyKey;
        await dbContext.SaveChangesAsync(cancellationToken);
        (int inserted, int updated) = type == ImportType.Products ? await CommitProductsAsync(operation.RowsJson, cancellationToken) : await CommitInventoryAsync(operation.RowsJson, cancellationToken);
        operation.Status = ImportStatus.Completed;
        operation.InsertedRows = inserted;
        operation.UpdatedRows = updated;
        operation.CompletedAtUtc = DateTime.UtcNow;
        operation.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.RecordAsync("import.committed", nameof(ImportOperation), operation.Id, $"{type} import committed: {inserted} inserted, {updated} updated.", cancellationToken);
        return new ImportCommitResponse(operation.Id, type, inserted, updated, false, operation.CompletedAtUtc.Value);
    }
}
