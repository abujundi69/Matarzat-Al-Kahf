using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace MatarzatAlKahf.Api.Authentication;

internal sealed class JwtAdminTokenService(JwtSettings settings) : IAdminTokenService
{
    private readonly JwtSettings _settings =
        settings ?? throw new ArgumentNullException(nameof(settings));

    public AccessToken CreateAccessToken(AdminTokenDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        DateTime issuedAtUtc = DateTime.UtcNow;
        DateTime expiresAtUtc = issuedAtUtc.AddMinutes(_settings.AccessTokenMinutes);
        string tokenId = Guid.NewGuid().ToString("N");
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, descriptor.AdminUserId.ToString()),
            new(ClaimTypes.NameIdentifier, descriptor.AdminUserId.ToString()),
            new(AdminClaimTypes.AdminUserId, descriptor.AdminUserId.ToString()),
            new(JwtRegisteredClaimNames.PhoneNumber, descriptor.PhoneNumber),
            new(ClaimTypes.Name, descriptor.FullName),
            new(
                AdminClaimTypes.PreferredLanguage,
                descriptor.PreferredLanguage == LanguageCode.English ? "en" : "ar"),
            new(JwtRegisteredClaimNames.Jti, tokenId),
        };

        claims.AddRange(descriptor.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(descriptor.Permissions.Select(
            permission => new Claim(AdminClaimTypes.Permission, permission)));

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return new AccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            tokenId,
            expiresAtUtc);
    }
}

internal sealed class BootstrapDisabledAdminTokenService : IAdminTokenService
{
    public AccessToken CreateAccessToken(AdminTokenDescriptor descriptor)
    {
        throw new InvalidOperationException(
            "JWT token creation is unavailable during administrator bootstrap mode.");
    }
}
