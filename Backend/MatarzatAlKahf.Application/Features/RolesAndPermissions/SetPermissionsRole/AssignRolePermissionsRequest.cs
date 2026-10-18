namespace MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Requests;

public sealed record AssignRolePermissionsRequest(IReadOnlyList<Guid> PermissionIds);
