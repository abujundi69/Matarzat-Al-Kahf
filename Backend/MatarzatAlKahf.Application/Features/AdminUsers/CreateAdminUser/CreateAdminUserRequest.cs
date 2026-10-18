using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;

public sealed record CreateAdminUserRequest(
    string FullName,
    string PhoneNumber,
    string? Email,
    string Password,
    LanguageCode PreferredLanguage,
    IReadOnlyList<Guid>? RoleIds = null);
