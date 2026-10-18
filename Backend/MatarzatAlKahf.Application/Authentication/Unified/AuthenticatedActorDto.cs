using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Authentication.Unified;

public sealed record AuthenticatedActorDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    string? Email,
    LanguageCode PreferredLanguage,
    string ActorType,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
