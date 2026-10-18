namespace MatarzatAlKahf.Application.Features.Payments.DTOs.Requests;

public sealed record RefundPaymentRequest(decimal? Amount, string Reason);
