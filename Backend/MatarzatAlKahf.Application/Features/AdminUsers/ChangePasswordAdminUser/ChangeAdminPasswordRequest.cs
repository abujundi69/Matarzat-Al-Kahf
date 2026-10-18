namespace MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;

public sealed record ChangeAdminPasswordRequest(
    string CurrentPassword,
    string NewPassword);
