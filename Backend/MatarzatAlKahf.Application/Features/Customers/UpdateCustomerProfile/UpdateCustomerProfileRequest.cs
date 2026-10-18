using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;

public sealed record UpdateCustomerProfileRequest(
    string FullName,
    string? Email,
    LanguageCode PreferredLanguage);
