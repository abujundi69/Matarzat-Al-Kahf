using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;

public sealed record UpdateAdminUserRequest(
    string FullName,
    string PhoneNumber,
    string? Email,
    LanguageCode PreferredLanguage);
