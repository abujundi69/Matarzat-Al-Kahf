using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Infrastructure.Features.CustomerAuthentication.Services;

public sealed class DisabledOtpDeliveryService : IOtpDeliveryService
{
    public Task DeliverAsync(
        string phoneNumber,
        string code,
        OtpPurpose purpose,
        LanguageCode language,
        CancellationToken cancellationToken) =>
        throw new ServiceUnavailableException(
            "OTP delivery is not configured. Configure an IOtpDeliveryService provider.");
}
