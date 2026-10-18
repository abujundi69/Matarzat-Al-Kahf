using System.Linq.Expressions;
using System.Net.Mail;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.Interfaces;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Customers.Services;
public sealed partial class AdminCustomerService
{
    public async Task<IReadOnlyList<CustomerAddressResponse>> GetAddressesAsync(Guid id, CancellationToken cancellationToken)
    {
        _ = await GetAsync(id, cancellationToken);
        return await dbContext.CustomerAddresses.AsNoTracking().Where(address => address.CustomerId == id && !address.IsDeleted).OrderByDescending(address => address.IsDefault).Select(address => new CustomerAddressResponse(address.Id, address.DeliveryLocationId, address.Type, address.City, address.Area, address.AddressLine, address.TaxNumber == string.Empty ? null : address.TaxNumber, address.DeliveryNotes == string.Empty ? null : address.DeliveryNotes, address.IsDefault)).ToListAsync(cancellationToken);
    }
}
