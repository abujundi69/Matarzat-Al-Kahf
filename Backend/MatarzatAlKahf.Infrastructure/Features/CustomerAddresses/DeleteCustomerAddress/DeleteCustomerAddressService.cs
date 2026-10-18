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
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        CustomerAddress address = await FindAsync(id, true, cancellationToken);
        bool wasDefault = address.IsDefault;
        address.IsDeleted = true;
        address.IsDefault = false;
        address.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        if (wasDefault)
        {
            CustomerAddress? replacement = await dbContext.CustomerAddresses.Where(candidate => candidate.CustomerId == address.CustomerId && !candidate.IsDeleted).OrderBy(candidate => candidate.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
            if (replacement is not null)
            {
                replacement.IsDefault = true;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
