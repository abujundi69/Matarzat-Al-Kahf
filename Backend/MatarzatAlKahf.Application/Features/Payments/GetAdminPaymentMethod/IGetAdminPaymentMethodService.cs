using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.Interfaces;
public interface IGetAdminPaymentMethodService
{
    Task<IReadOnlyList<PaymentMethodResponse>> GetAdminAsync(CancellationToken cancellationToken);
}
