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
    public async Task<AdminInventoryDto> GetByProductVariantIdAsync(Guid productVariantId, CancellationToken cancellationToken)
    {
        LanguageCode language = _languageResolver.GetCurrentLanguage();
        InventoryReadRow? row = await InventoryRows(productVariantId).SingleOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            throw new NotFoundException(nameof(InventoryItem), productVariantId);
        }

        string productName = await _dbContext.ProductTranslations.AsNoTracking().Where(translation => translation.ProductId == row.ProductId && !translation.IsDeleted).OrderBy(translation => translation.Language == language ? 0 : translation.Language == LanguageCode.Arabic ? 1 : 2).Select(translation => translation.Name).FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
        return Map(row, productName);
    }
}
