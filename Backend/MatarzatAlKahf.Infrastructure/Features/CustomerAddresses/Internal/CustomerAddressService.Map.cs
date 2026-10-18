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
    private static CustomerAddressResponse Map(CustomerAddress address) => new(address.Id, address.DeliveryLocationId, address.Type, address.City, address.Area, address.AddressLine, string.IsNullOrEmpty(address.TaxNumber) ? null : address.TaxNumber, string.IsNullOrEmpty(address.DeliveryNotes) ? null : address.DeliveryNotes, address.IsDefault);
}
