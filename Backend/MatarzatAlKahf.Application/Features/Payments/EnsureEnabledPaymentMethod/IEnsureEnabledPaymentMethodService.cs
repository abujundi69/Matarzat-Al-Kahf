using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.Interfaces;
public interface IEnsureEnabledPaymentMethodService
{
    Task EnsureEnabledAsync(PaymentMethod method, CancellationToken cancellationToken);
}
