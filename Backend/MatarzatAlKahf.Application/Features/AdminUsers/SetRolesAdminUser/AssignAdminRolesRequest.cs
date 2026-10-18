namespace MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;

public sealed record AssignAdminRolesRequest(IReadOnlyList<Guid> RoleIds);
