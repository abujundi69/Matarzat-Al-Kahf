using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/permissions")]
[Authorize(Policy = PermissionCodes.AdminRolesRead)]
[Tags("Roles and Permissions")]
public sealed class PermissionsController(
    IPermissionService permissionService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PermissionResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PermissionResponse>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await permissionService.ListAsync(cancellationToken));
    }
}
