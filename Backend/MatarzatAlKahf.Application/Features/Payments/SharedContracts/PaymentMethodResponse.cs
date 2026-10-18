using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.DTOs;
public sealed record PaymentMethodResponse(PaymentMethod Method, bool IsEnabled, string Instructions);
