using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.Interfaces;
public interface IOnlinePaymentGateway
{
    bool IsConfigured { get; }

    Task<OnlinePaymentResult> StartAsync(OnlinePaymentRequest request, CancellationToken cancellationToken);
}
