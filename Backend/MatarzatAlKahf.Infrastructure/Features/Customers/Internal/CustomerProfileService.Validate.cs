using System.Net.Mail;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Customers.Interfaces;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Customers.Services;
public sealed partial class CustomerProfileService
{
    private static void Validate(UpdateCustomerProfileRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length > 200)
        {
            errors[nameof(request.FullName)] = ["Full name is required and cannot exceed 200 characters."];
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && (request.Email.Trim().Length > 320 || !MailAddress.TryCreate(request.Email.Trim(), out _)))
        {
            errors[nameof(request.Email)] = ["Email address is invalid."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Customer profile is invalid.", errors);
        }
    }
}
