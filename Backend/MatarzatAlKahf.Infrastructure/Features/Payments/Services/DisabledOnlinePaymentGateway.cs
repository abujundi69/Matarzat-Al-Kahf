using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;

namespace MatarzatAlKahf.Infrastructure.Features.Payments.Services;

public sealed class DisabledOnlinePaymentGateway : IOnlinePaymentGateway
{
    public bool IsConfigured => false;

    public Task<OnlinePaymentResult> StartAsync(
        OnlinePaymentRequest request,
        CancellationToken cancellationToken) =>
        throw new ServiceUnavailableException(
            "Online payment is unavailable because no verified gateway is configured.");
}
