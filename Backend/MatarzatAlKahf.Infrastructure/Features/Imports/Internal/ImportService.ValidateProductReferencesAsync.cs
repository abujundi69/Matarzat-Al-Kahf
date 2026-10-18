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
    private async Task ValidateProductReferencesAsync(IReadOnlyList<ProductImportRow> rows, List<ImportRowError> errors, CancellationToken cancellationToken)
    {
        Guid[] categoryIds = rows.Select(row => row.CategoryId).Where(id => id != Guid.Empty).Distinct().ToArray();
        HashSet<Guid> existingCategories = (await dbContext.Categories.AsNoTracking().Where(category => categoryIds.Contains(category.Id) && !category.IsDeleted).Select(category => category.Id).ToListAsync(cancellationToken)).ToHashSet();
        string[] skus = rows.Select(row => row.Sku).Where(sku => sku.Length > 0).Distinct().ToArray();
        HashSet<string> existingSkus = (await dbContext.ProductVariants.AsNoTracking().Where(variant => skus.Contains(variant.Sku) && !variant.IsDeleted).Select(variant => variant.Sku).ToListAsync(cancellationToken)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (ProductImportRow row in rows)
        {
            if (!existingCategories.Contains(row.CategoryId))
            {
                errors.Add(new ImportRowError(row.RowNumber, "CategoryId", "Category does not exist."));
            }

            if (!row.AllowNew && !existingSkus.Contains(row.Sku))
            {
                errors.Add(new ImportRowError(row.RowNumber, "SKU", "SKU does not exist and adding products was not permitted."));
            }
        }
    }
}
