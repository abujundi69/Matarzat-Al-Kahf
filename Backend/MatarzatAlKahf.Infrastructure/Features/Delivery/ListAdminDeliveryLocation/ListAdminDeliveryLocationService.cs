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
    public async Task<IReadOnlyList<AdminDeliveryLocationResponse>> ListAdminAsync(CancellationToken cancellationToken)
    {
        List<DeliveryLocation> locations = await dbContext.DeliveryLocations.AsNoTracking().Where(location => !location.IsDeleted).OrderBy(location => location.SortOrder).ToListAsync(cancellationToken);
        return locations.Select(Map).ToArray();
    }
}
