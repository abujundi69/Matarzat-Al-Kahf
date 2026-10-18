namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;

public sealed record ResetCustomerPasswordRequest(
    string PhoneNumber,
    string VerificationToken,
    string NewPassword);
