using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Authentication.Admin;

public sealed record AccessToken(string Value, string TokenId, DateTime ExpiresAtUtc)
{
    public const string BearerTokenType = "Bearer";
}
