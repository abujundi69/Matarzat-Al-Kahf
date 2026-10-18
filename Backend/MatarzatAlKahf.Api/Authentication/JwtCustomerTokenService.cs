using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace MatarzatAlKahf.Api.Authentication;

internal sealed class JwtCustomerTokenService(
    JwtSettings settings) : ICustomerTokenService
{
    public CustomerAccessToken CreateAccessToken(CustomerTokenDescriptor descriptor)
    {
        DateTime issuedAt = DateTime.UtcNow;
        DateTime expires = issuedAt.AddMinutes(settings.AccessTokenMinutes);
        string tokenId = Guid.NewGuid().ToString("N");
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, descriptor.CustomerId.ToString()),
            new(ClaimTypes.NameIdentifier, descriptor.CustomerId.ToString()),
            new(CustomerClaimTypes.CustomerId, descriptor.CustomerId.ToString()),
            new(JwtRegisteredClaimNames.PhoneNumber, descriptor.PhoneNumber),
            new(ClaimTypes.Name, descriptor.FullName),
            new(
                AdminClaimTypes.PreferredLanguage,
                descriptor.PreferredLanguage == LanguageCode.English ? "en" : "ar"),
            new(JwtRegisteredClaimNames.Jti, tokenId)
        ];
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            issuedAt,
            expires,
            credentials);
        return new CustomerAccessToken(
            new JwtSecurityTokenHandler().WriteToken(jwt),
            tokenId,
            expires);
    }
}
