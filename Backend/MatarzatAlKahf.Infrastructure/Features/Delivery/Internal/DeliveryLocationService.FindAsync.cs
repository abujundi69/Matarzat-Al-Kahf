using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Features.Delivery.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Delivery.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Delivery.Interfaces;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Delivery.Services;
public sealed partial class DeliveryLocationService
{
    private async Task<DeliveryLocation> FindAsync(Guid id, bool tracked, CancellationToken cancellationToken)
    {
        IQueryable<DeliveryLocation> query = dbContext.DeliveryLocations.Where(location => location.Id == id && !location.IsDeleted);
        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Delivery location was not found.");
    }
}
