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
    private async Task<PagedResult<AdminInventoryDto>> GetListAsync(bool lowStockOnly, int page, int pageSize, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(page, pageSize);
        LanguageCode language = _languageResolver.GetCurrentLanguage();
        IQueryable<InventoryReadRow> query = InventoryRows(lowStockOnly: lowStockOnly);
        int totalItems = await query.CountAsync(cancellationToken);
        List<InventoryReadRow> rows = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        Guid[] productIds = rows.Select(row => row.ProductId).Distinct().ToArray();
        List<ProductNameRow> names = await _dbContext.ProductTranslations.AsNoTracking().Where(translation => productIds.Contains(translation.ProductId) && !translation.IsDeleted).Select(translation => new ProductNameRow(translation.ProductId, translation.Language, translation.Name)).ToListAsync(cancellationToken);
        List<AdminInventoryDto> items = rows.Select(row => Map(row, names.Where(name => name.ProductId == row.ProductId).OrderBy(name => LanguagePriority(name.Language, language)).Select(name => name.Name).FirstOrDefault() ?? string.Empty)).OrderBy(inventory => inventory.ProductName, StringComparer.Ordinal).ThenBy(inventory => inventory.Sku, StringComparer.Ordinal).ToList();
        return PagedResultFactory.Create(items, page, pageSize, totalItems);
    }
}
