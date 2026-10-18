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
    private static void Apply(CustomerAddress address, SaveCustomerAddressRequest request)
    {
        address.Type = request.Type;
        address.DeliveryLocationId = request.DeliveryLocationId;
        address.City = request.City.Trim();
        address.Area = request.Area.Trim();
        address.AddressLine = request.AddressLine.Trim();
        address.TaxNumber = request.TaxNumber?.Trim() ?? string.Empty;
        address.DeliveryNotes = request.DeliveryNotes?.Trim() ?? string.Empty;
        address.IsDefault = request.IsDefault;
    }
}
