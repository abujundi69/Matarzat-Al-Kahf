using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Checkout.DTOs.Requests;

public sealed record CheckoutRequest(
    string? GuestCartToken,
    Guid? CustomerAddressId,
    Guid DeliveryLocationId,
    string CustomerName,
    string PhoneNumber,
    string Area,
    string AddressLine,
    string? TaxNumber,
    string? DeliveryNotes,
    PaymentMethod PaymentMethod,
    string? GuestPhoneVerificationToken,
    LanguageCode Language = LanguageCode.Arabic,
    string? PaymentReference = null);
