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
public sealed partial class ImportService(MatarzatAlKahfDbContext dbContext, ICurrentActor currentActor, ISkuNormalizer skuNormalizer, ISlugNormalizer slugNormalizer, IAuditService auditService) : IImportService
{
    private const int MaximumRows = 5000;
    private const long MaximumFileSize = 10 * 1024 * 1024;
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private sealed record ParsedRows(string RowsJson, IReadOnlyList<ImportRowError> Errors, int TotalRows);
    private sealed record ProductImportRow(int RowNumber, string Sku, Guid CategoryId, string ArabicName, string EnglishName, decimal Price, int Quantity, string? Size, string? Color, int ReorderLevel, bool AllowNew);
    private sealed record InventoryImportRow(int RowNumber, string Sku, int Quantity, int ReorderLevel);
}
