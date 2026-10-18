using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;
using MatarzatAlKahf.Application.Features.CustomerAddresses.Interfaces;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.CustomerAddresses.Services;
public sealed partial class CustomerAddressService
{
    private async Task EnsureLocationAsync(Guid id, CancellationToken cancellationToken)
    {
        bool exists = await dbContext.DeliveryLocations.AsNoTracking().AnyAsync(location => location.Id == id && location.IsActive && !location.IsDeleted, cancellationToken);
        if (!exists)
        {
            throw ValidationException.For(nameof(SaveCustomerAddressRequest.DeliveryLocationId), "Delivery location is not supported.");
        }
    }
}
