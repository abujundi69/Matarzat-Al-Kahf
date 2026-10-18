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
    private Task<int> ClearDefaultsAsync(Guid customerId, CancellationToken cancellationToken) => dbContext.CustomerAddresses.Where(address => address.CustomerId == customerId && !address.IsDeleted && address.IsDefault).ExecuteUpdateAsync(updates => updates.SetProperty(address => address.IsDefault, false), cancellationToken);
}
