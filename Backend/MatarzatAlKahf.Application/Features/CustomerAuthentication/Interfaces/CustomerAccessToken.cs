using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
public sealed record CustomerAccessToken(string Value, string TokenId, DateTime ExpiresAtUtc);
