namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
public sealed record OtpVerificationResponse(string VerificationToken, DateTime ExpiresAtUtc);
