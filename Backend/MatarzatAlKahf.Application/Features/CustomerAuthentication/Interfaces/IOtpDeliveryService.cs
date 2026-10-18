using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;

public interface IOtpDeliveryService
{
    Task DeliverAsync(
        string phoneNumber,
        string code,
        OtpPurpose purpose,
        LanguageCode language,
        CancellationToken cancellationToken);
}
