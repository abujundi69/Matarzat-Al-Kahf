namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;

public sealed record CustomerLoginRequest(string PhoneNumber, string Password);
