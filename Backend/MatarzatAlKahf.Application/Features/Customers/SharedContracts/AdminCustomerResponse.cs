using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Customers.DTOs.Responses;

public sealed record AdminCustomerResponse(
    Guid Id,
    string FullName,
    string PhoneNumber,
    string? Email,
    UserStatus Status,
    bool IsPhoneVerified,
    LanguageCode PreferredLanguage,
    string? AdministrativeNotes,
    int AddressCount,
    int OrderCount,
    decimal TotalSpending,
    DateTime? LastLoginAtUtc,
    DateTime CreatedAtUtc);
