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
    public async Task<CustomerAddressResponse> UpdateAsync(Guid id, SaveCustomerAddressRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        await EnsureLocationAsync(request.DeliveryLocationId, cancellationToken);
        CustomerAddress address = await FindAsync(id, true, cancellationToken);
        if (request.IsDefault)
        {
            await ClearDefaultsAsync(address.CustomerId, cancellationToken);
        }

        Apply(address, request);
        address.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(address);
    }
}
