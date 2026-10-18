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
    public async Task<IReadOnlyList<CustomerAddressResponse>> ListAsync(CancellationToken cancellationToken)
    {
        Guid customerId = GetCustomerId();
        List<CustomerAddress> addresses = await dbContext.CustomerAddresses.AsNoTracking().Where(address => address.CustomerId == customerId && !address.IsDeleted).OrderByDescending(address => address.IsDefault).ThenBy(address => address.CreatedAtUtc).ToListAsync(cancellationToken);
        return addresses.Select(Map).ToArray();
    }
}
