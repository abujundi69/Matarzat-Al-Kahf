namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;

public sealed record VerifyOtpRequest(string PhoneNumber, string Code);
