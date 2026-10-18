using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;

public sealed record CustomerAddressResponse(
    Guid Id,
    Guid DeliveryLocationId,
    AddressType Type,
    string City,
    string Area,
    string AddressLine,
    string? TaxNumber,
    string? DeliveryNotes,
    bool IsDefault);
