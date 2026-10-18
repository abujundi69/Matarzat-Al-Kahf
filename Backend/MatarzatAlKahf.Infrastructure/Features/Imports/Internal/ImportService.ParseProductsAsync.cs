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
    private async Task<ParsedRows> ParseProductsAsync(MemoryStream content, bool allowNew, CancellationToken cancellationToken)
    {
        List<string[]> raw = ReadRows(content);
        var rows = new List<ProductImportRow>();
        var errors = new List<ImportRowError>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int index = 1; index < raw.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int rowNumber = index + 1;
            string[] values = Pad(raw[index], 9);
            string sku = NormalizeSku(values[0], rowNumber, errors);
            if (!seen.Add(sku))
            {
                errors.Add(new ImportRowError(rowNumber, "SKU", "Duplicate SKU in file."));
            }

            _ = Guid.TryParse(values[1], out Guid categoryId);
            _ = decimal.TryParse(values[4], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price);
            _ = int.TryParse(values[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantity);
            _ = int.TryParse(values[8], NumberStyles.Integer, CultureInfo.InvariantCulture, out int reorder);
            if (categoryId == Guid.Empty)
            {
                errors.Add(new ImportRowError(rowNumber, "CategoryId", "Valid category ID is required."));
            }

            if (string.IsNullOrWhiteSpace(values[2]) || values[2].Length > 300)
            {
                errors.Add(new ImportRowError(rowNumber, "ArabicName", "Arabic name is invalid."));
            }

            if (string.IsNullOrWhiteSpace(values[3]) || values[3].Length > 300)
            {
                errors.Add(new ImportRowError(rowNumber, "EnglishName", "English name is invalid."));
            }

            if (price <= 0)
            {
                errors.Add(new ImportRowError(rowNumber, "Price", "Price must be positive."));
            }

            if (quantity < 0 || reorder < 0)
            {
                errors.Add(new ImportRowError(rowNumber, "Quantity", "Quantity and reorder level cannot be negative."));
            }

            rows.Add(new ProductImportRow(rowNumber, sku, categoryId, values[2].Trim(), values[3].Trim(), price, quantity, EmptyToNull(values[6]), EmptyToNull(values[7]), reorder, allowNew));
        }

        await ValidateProductReferencesAsync(rows, errors, cancellationToken);
        return new ParsedRows(JsonSerializer.Serialize(rows), errors, rows.Count);
    }
}
