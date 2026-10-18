using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Responses;
public sealed record AdminUserResponse(Guid Id, string FullName, string PhoneNumber, string? Email, UserStatus Status, LanguageCode PreferredLanguage, DateTime? LastLoginAtUtc, DateTime CreatedAtUtc, IReadOnlyList<AdminRoleSummaryResponse> Roles, IReadOnlyList<string> Permissions);
