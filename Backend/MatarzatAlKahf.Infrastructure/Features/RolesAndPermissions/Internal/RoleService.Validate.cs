using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Requests;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.Interfaces;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.RolesAndPermissions.Services;
public sealed partial class RoleService
{
    private static void Validate(SaveRoleRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        string? arabicDisplayName = request.DisplayNameArabic;
        string? englishDisplayName = request.DisplayNameEnglish;
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 200)
        {
            errors[nameof(request.Name)] = ["Name is required and cannot exceed 200 characters."];
        }

        bool hasArabicDisplayName = arabicDisplayName is not null;
        bool hasEnglishDisplayName = englishDisplayName is not null;
        if (hasArabicDisplayName != hasEnglishDisplayName)
        {
            errors[nameof(request.DisplayNameArabic)] =
                ["Arabic and English role display names must be provided together."];
            errors[nameof(request.DisplayNameEnglish)] =
                ["Arabic and English role display names must be provided together."];
        }
        else if (hasArabicDisplayName)
        {
            if (string.IsNullOrWhiteSpace(arabicDisplayName)
                || arabicDisplayName.Trim().Length > 200)
            {
                errors[nameof(request.DisplayNameArabic)] =
                    ["Arabic display name is required and cannot exceed 200 characters."];
            }

            if (string.IsNullOrWhiteSpace(englishDisplayName)
                || englishDisplayName.Trim().Length > 200)
            {
                errors[nameof(request.DisplayNameEnglish)] =
                    ["English display name is required and cannot exceed 200 characters."];
            }
        }

        if (request.Description?.Trim().Length > 4000)
        {
            errors[nameof(request.Description)] = ["Description cannot exceed 4000 characters."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Role data is invalid.", errors);
        }
    }
}
