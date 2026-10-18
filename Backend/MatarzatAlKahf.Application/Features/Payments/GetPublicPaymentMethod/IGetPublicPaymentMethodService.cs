using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.Interfaces;
public interface IGetPublicPaymentMethodService
{
    Task<IReadOnlyList<PaymentMethodResponse>> GetPublicAsync(CancellationToken cancellationToken);
}
