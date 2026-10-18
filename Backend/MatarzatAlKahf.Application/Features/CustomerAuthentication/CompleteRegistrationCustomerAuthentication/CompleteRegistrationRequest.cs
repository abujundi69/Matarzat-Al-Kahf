using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;

public sealed record CompleteRegistrationRequest(
    string VerificationToken,
    string FullName,
    string PhoneNumber,
    string? Email,
    string Password,
    LanguageCode PreferredLanguage = LanguageCode.Arabic);
