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
    public async Task<AdminProfileDto> GetProfileAsync(CancellationToken cancellationToken)
    {
        AdminUser user = await GetCurrentAsync(cancellationToken);
        AdminUserResponse response = Map(user);
        return new AdminProfileDto(response.Id, response.FullName, response.PhoneNumber, response.Email, response.PreferredLanguage, response.Roles.Select(role => role.Name).ToArray(), response.Permissions)
        {
            RoleSummaries = response.Roles
        };
    }
}
