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
    public async Task<CustomerAddressResponse> CreateAsync(SaveCustomerAddressRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        Guid customerId = GetCustomerId();
        await EnsureLocationAsync(request.DeliveryLocationId, cancellationToken);
        bool hasAddresses = await dbContext.CustomerAddresses.AnyAsync(address => address.CustomerId == customerId && !address.IsDeleted, cancellationToken);
        if (request.IsDefault || !hasAddresses)
        {
            await ClearDefaultsAsync(customerId, cancellationToken);
        }

        var address = new CustomerAddress
        {
            CustomerId = customerId
        };
        Apply(address, request);
        address.IsDefault = request.IsDefault || !hasAddresses;
        dbContext.CustomerAddresses.Add(address);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(address);
    }
}
