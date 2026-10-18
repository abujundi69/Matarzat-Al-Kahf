using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Responses;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Authentication.Admin;
public sealed record AdminProfileDto(Guid Id, string FullName, string PhoneNumber, string? Email, LanguageCode PreferredLanguage, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions)
{
    public IReadOnlyList<AdminRoleSummaryResponse> RoleSummaries { get; init; } = [];
}
