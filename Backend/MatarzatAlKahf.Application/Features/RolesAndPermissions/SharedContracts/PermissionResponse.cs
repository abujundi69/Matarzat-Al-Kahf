namespace MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;

public sealed record PermissionResponse(
    Guid Id,
    string Code,
    string Name,
    string Description);
