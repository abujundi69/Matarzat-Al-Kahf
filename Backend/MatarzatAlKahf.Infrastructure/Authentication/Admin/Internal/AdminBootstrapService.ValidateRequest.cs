using System.Data;
using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Authentication.Admin;
public sealed partial class AdminBootstrapService
{
    private void ValidateRequest(BootstrapAdminRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            errors[nameof(request.FullName)] = ["Full name is required."];
        }
        else if (request.FullName.Trim().Length > 200)
        {
            errors[nameof(request.FullName)] = ["Full name cannot exceed 200 characters."];
        }

        if (request.Email?.Trim().Length > 320)
        {
            errors[nameof(request.Email)] = ["Email cannot exceed 320 characters."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Bootstrap administrator data is invalid.", errors);
        }

        _passwordPolicy.Validate(request.Password);
    }
}
