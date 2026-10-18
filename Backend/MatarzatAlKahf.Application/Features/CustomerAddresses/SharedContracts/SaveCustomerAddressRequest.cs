using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Requests;

public sealed record SaveCustomerAddressRequest(
    Guid DeliveryLocationId,
    AddressType Type,
    string City,
    string Area,
    string AddressLine,
    string? TaxNumber,
    string? DeliveryNotes,
    bool IsDefault);
