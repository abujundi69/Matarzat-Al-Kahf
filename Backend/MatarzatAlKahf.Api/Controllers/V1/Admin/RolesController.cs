using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Requests;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/roles")]
[Tags("Roles and Permissions")]
public sealed class RolesController(IListRoleService listRoleService, IGetRoleService getRoleService, ICreateRoleService createRoleService, IUpdateRoleService updateRoleService, IDeleteRoleService deleteRoleService, ISetPermissionsRoleService setPermissionsRoleService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.AdminRolesRead)]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> List(CancellationToken cancellationToken) => Ok(await listRoleService.ListAsync(cancellationToken));
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.AdminRolesRead)]
    public async Task<ActionResult<RoleResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getRoleService.GetAsync(id, cancellationToken));
    [HttpPost]
    [Authorize(Policy = PermissionCodes.AdminRolesWrite)]
    public async Task<ActionResult<RoleResponse>> Create(SaveRoleRequest request, CancellationToken cancellationToken)
    {
        RoleResponse response = await createRoleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.AdminRolesWrite)]
    public async Task<ActionResult<RoleResponse>> Update(Guid id, SaveRoleRequest request, CancellationToken cancellationToken) => Ok(await updateRoleService.UpdateAsync(id, request, cancellationToken));
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCodes.AdminRolesWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteRoleService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize(Policy = PermissionCodes.AdminRolesWrite)]
    public async Task<ActionResult<RoleResponse>> SetPermissions(Guid id, AssignRolePermissionsRequest request, CancellationToken cancellationToken) => Ok(await setPermissionsRoleService.SetPermissionsAsync(id, request, cancellationToken));
}
