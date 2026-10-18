using System.Text.Json;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Api.Authentication;

internal sealed class DevelopmentOtpDeliveryService(IConfiguration configuration)
    : IOtpDeliveryService
{
    public async Task DeliverAsync(
        string phoneNumber,
        string code,
        OtpPurpose purpose,
        LanguageCode language,
        CancellationToken cancellationToken)
    {
        string path = configuration["Otp:TestCapturePath"]
            ?? throw new InvalidOperationException(
                "Otp:TestCapturePath is required for the development test provider.");
        if (!Path.IsPathFullyQualified(path))
        {
            throw new InvalidOperationException("Otp:TestCapturePath must be an absolute path.");
        }

        string payload = JsonSerializer.Serialize(new
        {
            phoneNumber,
            code,
            purpose,
            language,
            capturedAtUtc = DateTime.UtcNow
        });
        await File.WriteAllTextAsync(path, payload, cancellationToken);
    }
}
