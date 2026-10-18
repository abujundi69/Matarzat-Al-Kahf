namespace MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;

public sealed record ChangeCustomerPasswordRequest(
    string CurrentPassword,
    string NewPassword);
