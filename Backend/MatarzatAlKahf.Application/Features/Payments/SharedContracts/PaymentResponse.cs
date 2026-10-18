using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.DTOs.Responses;

public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    PaymentMethod Method,
    PaymentStatus Status,
    decimal Amount,
    CurrencyCode Currency,
    string? TransactionReference,
    string? ProofUrl,
    string? VerificationNote,
    DateTime? PaidAtUtc,
    DateTime CreatedAtUtc);
