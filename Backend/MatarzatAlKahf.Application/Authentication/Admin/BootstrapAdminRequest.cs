namespace MatarzatAlKahf.Application.Authentication.Admin;
public sealed record BootstrapAdminRequest(string FullName, string PhoneNumber, string? Email, string Password);
