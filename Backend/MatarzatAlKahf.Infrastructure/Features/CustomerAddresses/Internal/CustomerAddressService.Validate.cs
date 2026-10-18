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
    private static void Validate(SaveCustomerAddressRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        Check(request.City, 150, nameof(request.City), errors);
        Check(request.Area, 150, nameof(request.Area), errors);
        Check(request.AddressLine, 1000, nameof(request.AddressLine), errors);
        if (request.TaxNumber?.Trim().Length > 100)
        {
            errors[nameof(request.TaxNumber)] = ["Tax number cannot exceed 100 characters."];
        }

        if (request.DeliveryNotes?.Trim().Length > 2000)
        {
            errors[nameof(request.DeliveryNotes)] = ["Delivery notes cannot exceed 2000 characters."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Address data is invalid.", errors);
        }
    }
}
