using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;

public sealed record UpdateAdminCustomerRequest(
    string FullName,
    string? Email,
    LanguageCode PreferredLanguage,
    string? AdministrativeNotes);
