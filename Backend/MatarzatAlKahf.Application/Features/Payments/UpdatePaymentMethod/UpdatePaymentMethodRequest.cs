using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.DTOs;
public sealed record UpdatePaymentMethodRequest(bool IsEnabled, string ArabicInstructions, string EnglishInstructions);
