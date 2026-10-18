using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
public sealed record CustomerLoginResponse(string AccessToken, string TokenType, DateTime ExpiresAtUtc, CustomerProfileResponse Customer);
