namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
public sealed record OtpRequestResponse(DateTime ExpiresAtUtc, int ResendAfterSeconds);
