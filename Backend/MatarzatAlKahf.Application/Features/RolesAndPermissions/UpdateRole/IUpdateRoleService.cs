using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Requests;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.RolesAndPermissions.Interfaces;
public interface IUpdateRoleService
{
    Task<RoleResponse> UpdateAsync(Guid id, SaveRoleRequest request, CancellationToken cancellationToken);
}
