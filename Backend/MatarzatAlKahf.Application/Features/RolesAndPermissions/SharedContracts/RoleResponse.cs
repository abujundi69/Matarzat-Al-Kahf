namespace MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;

public sealed record RoleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystemRole,
    int AssignedUserCount,
    IReadOnlyList<PermissionResponse> Permissions)
{
    public string? DisplayNameArabic { get; init; }

    public string? DisplayNameEnglish { get; init; }
}
