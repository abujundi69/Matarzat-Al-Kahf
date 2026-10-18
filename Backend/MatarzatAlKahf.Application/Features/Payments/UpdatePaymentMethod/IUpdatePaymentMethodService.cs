using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.Interfaces;
public interface IUpdatePaymentMethodService
{
    Task<PaymentMethodResponse> UpdateAsync(PaymentMethod method, UpdatePaymentMethodRequest request, CancellationToken cancellationToken);
}
