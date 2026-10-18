using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Application.Inventory;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Inventory;
public sealed partial class AdminInventoryService
{
    public async Task<InventoryAdjustmentResultDto> AdjustAsync(Guid productVariantId, InventoryAdjustmentRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        string sku = string.Empty;
        try
        {
            InventoryAdjustmentResultDto result = await DbTransactionExecutor.ExecuteAsync(_dbContext, async token =>
            {
                InventoryAdjustmentRow? inventory = await _dbContext.InventoryItems.AsNoTracking().Where(entity => entity.ProductVariantId == productVariantId && !entity.IsDeleted && !entity.ProductVariant.IsDeleted && !entity.ProductVariant.Product.IsDeleted).Select(entity => new InventoryAdjustmentRow(entity.Id, entity.ProductVariant.Sku, entity.QuantityOnHand, entity.ReservedQuantity, entity.ReorderLevel)).SingleOrDefaultAsync(token) ?? throw new NotFoundException(nameof(InventoryItem), productVariantId);
                sku = inventory.Sku;
                _validator.Validate(request, inventory.QuantityOnHand);
                byte[] submittedRowVersion;
                try
                {
                    submittedRowVersion = Convert.FromBase64String(request.RowVersion);
                }
                catch (FormatException exception)
                {
                    throw ValidationException.For(nameof(request.RowVersion), $"RowVersion must be valid Base64. ({exception.GetType().Name})");
                }

                long newQuantity = (long)inventory.QuantityOnHand + request.QuantityChange;
                if (newQuantity > int.MaxValue)
                {
                    throw new ConflictException("Inventory quantity exceeds the supported maximum.");
                }

                DateTime updatedAtUtc = DateTime.UtcNow;
                int affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
                        UPDATE [catalog].[InventoryItems]
                        SET [QuantityOnHand] = {(int)newQuantity},
                            [UpdatedAtUtc] = {updatedAtUtc}
                        WHERE [Id] = {inventory.Id}
                          AND [RowVersion] = {submittedRowVersion}
                        """, token);
                if (affectedRows != 1)
                {
                    LogConcurrencyConflict(_logger, productVariantId, null);
                    throw new ConcurrencyConflictException();
                }

                _dbContext.InventoryTransactions.Add(new InventoryTransaction { InventoryItemId = inventory.Id, Type = request.Type, Quantity = request.QuantityChange, Reference = NormalizeOptional(request.Reference), Notes = NormalizeOptional(request.Notes) });
                await _dbContext.SaveChangesAsync(token);
                byte[] newRowVersion = await _dbContext.InventoryItems.AsNoTracking().Where(entity => entity.Id == inventory.Id).Select(entity => entity.RowVersion).SingleAsync(token);
                int available = (int)newQuantity - inventory.ReservedQuantity;
                return new InventoryAdjustmentResultDto(productVariantId, (int)newQuantity, inventory.ReservedQuantity, available, inventory.ReorderLevel, available <= inventory.ReorderLevel, Convert.ToBase64String(newRowVersion));
            }, cancellationToken);
            LogInventoryAdjusted(_logger, productVariantId, request.QuantityChange, request.Type, null);
            if (result.AvailableQuantity <= result.ReorderLevel)
            {
                await _notificationService.EnqueueAsync(new EnqueueNotificationRequest(null, null, null, LanguageCode.Arabic, NotificationChannel.InApp, result.AvailableQuantity <= 0 ? "Out of stock" : "Low stock", $"Variant {sku} has {result.AvailableQuantity} available.", "administrators"), cancellationToken);
            }

            return result;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            LogConcurrencyConflict(_logger, productVariantId, exception);
            throw new ConcurrencyConflictException("Inventory was changed by another request. Reload it and try again.", exception);
        }
    }
}
