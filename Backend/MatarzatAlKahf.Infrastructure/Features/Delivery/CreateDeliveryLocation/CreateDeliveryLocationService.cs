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
    public async Task<AdminDeliveryLocationResponse> CreateAsync(SaveDeliveryLocationRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var location = new DeliveryLocation();
        Apply(location, request);
        dbContext.DeliveryLocations.Add(location);
        await SaveAsync(cancellationToken);
        await AuditAsync("delivery.location.created", location, cancellationToken);
        return Map(location);
    }
}
