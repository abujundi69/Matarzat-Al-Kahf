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
    private async Task<(int Inserted, int Updated)> CommitProductsAsync(string json, CancellationToken cancellationToken)
    {
        ProductImportRow[] rows = JsonSerializer.Deserialize<ProductImportRow[]>(json) ?? [];
        string[] skus = rows.Select(row => row.Sku).ToArray();
        Dictionary<string, ProductVariant> variants = (await dbContext.ProductVariants.Include(variant => variant.Product).ThenInclude(product => product.Translations).Include(variant => variant.InventoryItem).ThenInclude(item => item.Transactions).Where(variant => skus.Contains(variant.Sku) && !variant.IsDeleted).ToListAsync(cancellationToken)).ToDictionary(variant => variant.Sku, StringComparer.OrdinalIgnoreCase);
        int inserted = 0;
        int updated = 0;
        foreach (ProductImportRow row in rows)
        {
            if (variants.TryGetValue(row.Sku, out ProductVariant? variant))
            {
                variant.Product.CategoryId = row.CategoryId;
                variant.Product.BasePrice = row.Price;
                variant.Size = row.Size;
                variant.Color = row.Color;
                UpdateTranslation(variant.Product, LanguageCode.Arabic, row.ArabicName);
                UpdateTranslation(variant.Product, LanguageCode.English, row.EnglishName);
                SetInventory(variant.InventoryItem, row.Quantity, row.ReorderLevel, "Product import");
                updated++;
                continue;
            }

            string suffix = Guid.NewGuid().ToString("N")[..8];
            var product = new Product
            {
                CategoryId = row.CategoryId,
                BasePrice = row.Price,
                Currency = CurrencyCode.ILS,
                Status = ProductStatus.Active,
                PublishedAtUtc = DateTime.UtcNow,
                Translations = [CreateTranslation(LanguageCode.Arabic, row.ArabicName, $"{slugNormalizer.Normalize(row.ArabicName)}-{suffix}"), CreateTranslation(LanguageCode.English, row.EnglishName, $"{slugNormalizer.Normalize(row.EnglishName)}-{suffix}")]
            };
            product.Variants.Add(new ProductVariant { Sku = row.Sku, Size = row.Size, Color = row.Color, IsActive = true, InventoryItem = new InventoryItem { QuantityOnHand = row.Quantity, ReorderLevel = row.ReorderLevel, Transactions = [new InventoryTransaction { Type = InventoryTransactionType.InitialStock, Quantity = row.Quantity, Reference = "Product import", Notes = "Created through confirmed product import." }] } });
            dbContext.Products.Add(product);
            inserted++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return (inserted, updated);
    }
}
