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
    public async Task<ImportCommitResponse> CommitAsync(Guid importId, ImportType type, CommitImportRequest request, string idempotencyKey, CancellationToken cancellationToken)
    {
        if (!request.Confirm)
        {
            throw ValidationException.For(nameof(request.Confirm), "Explicit confirmation is required.");
        }

        ValidateIdempotencyKey(idempotencyKey);
        Guid adminId = currentActor.AdminUserId ?? throw new AuthenticationFailedException("Administrator authentication is required.");
        return await DbTransactionExecutor.ExecuteAsync(dbContext, token => CommitCoreAsync(importId, type, adminId, idempotencyKey, token), cancellationToken, IsolationLevel.Serializable);
    }
}
