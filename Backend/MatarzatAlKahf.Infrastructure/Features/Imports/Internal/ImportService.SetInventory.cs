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
    private void SetInventory(InventoryItem inventory, int quantity, int reorderLevel, string reference)
    {
        int difference = quantity - inventory.QuantityOnHand;
        if (quantity < inventory.ReservedQuantity)
        {
            throw new ConflictException("Imported quantity cannot be lower than the reserved quantity.");
        }

        inventory.QuantityOnHand = quantity;
        inventory.ReorderLevel = reorderLevel;
        inventory.UpdatedAtUtc = DateTime.UtcNow;
        if (difference != 0)
        {
            var transaction = new InventoryTransaction
            {
                Type = difference > 0 ? InventoryTransactionType.AdjustmentIncrease : InventoryTransactionType.AdjustmentDecrease,
                Quantity = Math.Abs(difference),
                Reference = reference,
                Notes = "Quantity set by confirmed import."
            };
            inventory.Transactions.Add(transaction);
            dbContext.InventoryTransactions.Add(transaction);
        }
    }
}
