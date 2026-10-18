using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
public sealed record CustomerProfileResponse(Guid Id, string FullName, string PhoneNumber, string? Email, LanguageCode PreferredLanguage);
