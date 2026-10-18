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
    private static void Validate(UpdateAdminCustomerRequest request)
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

        if (request.AdministrativeNotes?.Length > 4000)
        {
            errors[nameof(request.AdministrativeNotes)] = ["Administrative notes cannot exceed 4000 characters."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Customer data is invalid.", errors);
        }
    }
}
