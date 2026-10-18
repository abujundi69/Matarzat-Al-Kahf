using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.Interfaces;
public sealed record OnlinePaymentRequest(Guid OrderId, string OrderNumber, decimal Amount, CurrencyCode Currency, string IdempotencyKey);
