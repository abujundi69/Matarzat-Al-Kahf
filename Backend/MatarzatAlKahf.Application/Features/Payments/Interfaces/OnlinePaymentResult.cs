using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.Interfaces;
public sealed record OnlinePaymentResult(string ProviderReference, string RedirectUrl);
