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
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        DeliveryLocation location = await FindAsync(id, true, cancellationToken);
        if (await dbContext.CustomerAddresses.AnyAsync(address => address.DeliveryLocationId == id && !address.IsDeleted, cancellationToken))
        {
            throw new ConflictException("A delivery location in use cannot be deleted.");
        }

        if (await dbContext.Orders.AnyAsync(order => order.DeliveryLocationId == id, cancellationToken))
        {
            throw new ConflictException("A delivery location used by existing orders cannot be deleted.");
        }

        location.IsDeleted = true;
        location.IsActive = false;
        location.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("delivery.location.deleted", location, cancellationToken);
    }
}
