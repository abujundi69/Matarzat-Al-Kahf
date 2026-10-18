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
    private async Task<(int Inserted, int Updated)> CommitInventoryAsync(string json, CancellationToken cancellationToken)
    {
        InventoryImportRow[] rows = JsonSerializer.Deserialize<InventoryImportRow[]>(json) ?? [];
        string[] skus = rows.Select(row => row.Sku).ToArray();
        Dictionary<string, InventoryItem> inventory = (await dbContext.InventoryItems.Include(item => item.ProductVariant).Include(item => item.Transactions).Where(item => skus.Contains(item.ProductVariant.Sku) && !item.IsDeleted).ToListAsync(cancellationToken)).ToDictionary(item => item.ProductVariant.Sku, StringComparer.OrdinalIgnoreCase);
        foreach (InventoryImportRow row in rows)
        {
            if (!inventory.TryGetValue(row.Sku, out InventoryItem? item))
            {
                throw new ConflictException($"SKU '{row.Sku}' no longer exists.");
            }

            SetInventory(item, row.Quantity, row.ReorderLevel, "Inventory import");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return (0, rows.Length);
    }
}
