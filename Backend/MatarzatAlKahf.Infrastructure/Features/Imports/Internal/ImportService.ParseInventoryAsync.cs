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
    private async Task<ParsedRows> ParseInventoryAsync(MemoryStream content, CancellationToken cancellationToken)
    {
        List<string[]> raw = ReadRows(content);
        var rows = new List<InventoryImportRow>();
        var errors = new List<ImportRowError>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int index = 1; index < raw.Count; index++)
        {
            int rowNumber = index + 1;
            string[] values = Pad(raw[index], 3);
            string sku = NormalizeSku(values[0], rowNumber, errors);
            if (!seen.Add(sku))
            {
                errors.Add(new ImportRowError(rowNumber, "SKU", "Duplicate SKU in file."));
            }

            bool quantityValid = int.TryParse(values[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantity);
            bool reorderValid = int.TryParse(values[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int reorder);
            if (!quantityValid || quantity < 0)
            {
                errors.Add(new ImportRowError(rowNumber, "Quantity", "Quantity is invalid."));
            }

            if (!reorderValid || reorder < 0)
            {
                errors.Add(new ImportRowError(rowNumber, "ReorderLevel", "Reorder level is invalid."));
            }

            rows.Add(new InventoryImportRow(rowNumber, sku, quantity, reorder));
        }

        string[] skus = rows.Select(row => row.Sku).Where(sku => sku.Length > 0).Distinct().ToArray();
        HashSet<string> existing = (await dbContext.ProductVariants.AsNoTracking().Where(variant => skus.Contains(variant.Sku) && !variant.IsDeleted).Select(variant => variant.Sku).ToListAsync(cancellationToken)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (InventoryImportRow row in rows.Where(row => !existing.Contains(row.Sku)))
        {
            errors.Add(new ImportRowError(row.RowNumber, "SKU", "SKU does not exist."));
        }

        return new ParsedRows(JsonSerializer.Serialize(rows), errors, rows.Count);
    }
}
