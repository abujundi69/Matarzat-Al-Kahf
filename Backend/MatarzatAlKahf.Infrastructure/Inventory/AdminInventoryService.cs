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
public sealed partial class AdminInventoryService(MatarzatAlKahfDbContext dbContext, IActiveLanguageResolver languageResolver, IInventoryAdjustmentValidator validator, INotificationService notificationService, ILogger<AdminInventoryService> logger) : IAdminInventoryService
{
    private static readonly Action<ILogger, Guid, Exception?> LogConcurrencyConflict = LoggerMessage.Define<Guid>(LogLevel.Warning, new EventId(4001, nameof(LogConcurrencyConflict)), "Inventory concurrency conflict for variant {ProductVariantId}");
    private static readonly Action<ILogger, Guid, int, InventoryTransactionType, Exception?> LogInventoryAdjusted = LoggerMessage.Define<Guid, int, InventoryTransactionType>(LogLevel.Information, new EventId(4002, nameof(LogInventoryAdjusted)), "Inventory for variant {ProductVariantId} adjusted by {QuantityChange} using {TransactionType}");
    private readonly MatarzatAlKahfDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IActiveLanguageResolver _languageResolver = languageResolver ?? throw new ArgumentNullException(nameof(languageResolver));
    private readonly IInventoryAdjustmentValidator _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly INotificationService _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    private readonly ILogger<AdminInventoryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private sealed record InventoryReadRow(Guid ProductVariantId, Guid ProductId, string Sku, string? Size, string? Color, int QuantityOnHand, int ReservedQuantity, int ReorderLevel, byte[] RowVersion);
    private sealed record ProductNameRow(Guid ProductId, LanguageCode Language, string Name);
    private sealed record InventoryAdjustmentRow(Guid Id, string Sku, int QuantityOnHand, int ReservedQuantity, int ReorderLevel);
}
