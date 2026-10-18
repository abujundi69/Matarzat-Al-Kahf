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
    public Task<PagedResult<AdminInventoryDto>> GetLowStockAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return GetListAsync(lowStockOnly: true, page, pageSize, cancellationToken);
    }
}
