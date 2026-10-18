using System.Text;

namespace MatarzatAlKahf.Api.Authentication;

internal sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public const int MinimumSigningKeySizeInBytes = 32;

    public string Issuer { get; init; } = "MatarzatAlKahf";

    public string Audience { get; init; } = "MatarzatAlKahf.Admin";

    public int AccessTokenMinutes { get; init; } = 60;

    public string SigningKey { get; init; } = string.Empty;

    public static JwtSettings FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var settings = new JwtSettings();
        configuration.GetSection(SectionName).Bind(settings);
        settings.Validate();

        return settings;
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("JWT issuer configuration is missing.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("JWT audience configuration is missing.");
        }

        if (AccessTokenMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT access-token lifetime must be greater than zero minutes.");
        }

        if (string.IsNullOrWhiteSpace(SigningKey)
            || Encoding.UTF8.GetByteCount(SigningKey) < MinimumSigningKeySizeInBytes)
        {
            throw new InvalidOperationException(
                "JWT signing key is missing or too short. Set Jwt__SigningKey to a secret "
                + $"containing at least {MinimumSigningKeySizeInBytes} UTF-8 bytes.");
        }
    }
}
