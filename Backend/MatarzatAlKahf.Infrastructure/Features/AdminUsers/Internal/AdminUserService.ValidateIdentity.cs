using System.Net.Mail;
using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Responses;
using MatarzatAlKahf.Application.Features.AdminUsers.Interfaces;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.AdminUsers.Services;
public sealed partial class AdminUserService
{
    private static void ValidateIdentity(string fullName, string? email)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > 200)
        {
            errors[nameof(fullName)] = ["Full name is required and cannot exceed 200 characters."];
        }

        if (!string.IsNullOrWhiteSpace(email) && (email.Trim().Length > 320 || !MailAddress.TryCreate(email.Trim(), out _)))
        {
            errors[nameof(email)] = ["Email address is invalid."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Administrator data is invalid.", errors);
        }
    }
}
