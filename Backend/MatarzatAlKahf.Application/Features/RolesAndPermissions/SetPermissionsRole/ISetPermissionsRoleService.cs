using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Requests;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.RolesAndPermissions.Interfaces;
public interface ISetPermissionsRoleService
{
    Task<RoleResponse> SetPermissionsAsync(Guid id, AssignRolePermissionsRequest request, CancellationToken cancellationToken);
}
