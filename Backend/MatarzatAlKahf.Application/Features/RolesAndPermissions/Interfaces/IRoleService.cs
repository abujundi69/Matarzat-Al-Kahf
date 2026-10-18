using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Requests;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.RolesAndPermissions.Interfaces;
public interface IRoleService : IListRoleService, IGetRoleService, ICreateRoleService, IUpdateRoleService, IDeleteRoleService, ISetPermissionsRoleService
{
}
