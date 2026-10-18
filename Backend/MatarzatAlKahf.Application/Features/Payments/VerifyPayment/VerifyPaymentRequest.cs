namespace MatarzatAlKahf.Application.Features.Payments.DTOs.Requests;

public sealed record VerifyPaymentRequest(string? TransactionReference, string? Note);
