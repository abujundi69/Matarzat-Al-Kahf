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
    private async Task<CustomerAddress> FindAsync(Guid id, bool tracked, CancellationToken cancellationToken)
    {
        Guid customerId = GetCustomerId();
        IQueryable<CustomerAddress> query = dbContext.CustomerAddresses.Where(address => address.Id == id && address.CustomerId == customerId && !address.IsDeleted);
        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Address was not found.");
    }
}
