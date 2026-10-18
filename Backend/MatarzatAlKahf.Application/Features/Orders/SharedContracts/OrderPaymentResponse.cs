using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
public sealed record OrderPaymentResponse(Guid Id, PaymentMethod Method, PaymentStatus Status, decimal Amount, CurrencyCode Currency, string? TransactionReference, string? ProofFilePath, DateTime CreatedAtUtc);
