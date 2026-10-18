using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.Interfaces;
public interface IGetCustomerOrderPaymentMethodService
{
    Task<PaymentMethodResponse> GetCustomerOrderAsync(Guid orderId, CancellationToken cancellationToken);
}
