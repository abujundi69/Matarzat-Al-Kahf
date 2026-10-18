namespace MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Requests;

public sealed record SaveRoleRequest(string Name, string Description)
{
    public string? DisplayNameArabic { get; init; }

    public string? DisplayNameEnglish { get; init; }
}
