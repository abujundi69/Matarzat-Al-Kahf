using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.Interfaces;
public interface IPaymentMethodService : IGetPublicPaymentMethodService, IGetAdminPaymentMethodService, IGetCustomerOrderPaymentMethodService, IUpdatePaymentMethodService, IEnsureEnabledPaymentMethodService
{
}
