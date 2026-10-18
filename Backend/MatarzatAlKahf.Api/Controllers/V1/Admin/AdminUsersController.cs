using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Responses;
using MatarzatAlKahf.Application.Features.AdminUsers.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/users")]
[Tags("Admin Users")]
public sealed class AdminUsersController(IListAdminUserService listAdminUserService, IGetAdminUserService getAdminUserService, ICreateAdminUserService createAdminUserService, IUpdateAdminUserService updateAdminUserService, ISetStatusAdminUserService setStatusAdminUserService, IDeleteAdminUserService deleteAdminUserService, IResetPasswordAdminUserService resetPasswordAdminUserService, ISetRolesAdminUserService setRolesAdminUserService, IGetPermissionsAdminUserService getPermissionsAdminUserService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.AdminUsersRead)]
    public async Task<ActionResult<PagedResult<AdminUserResponse>>> List([FromQuery] AdminUserQuery query, CancellationToken cancellationToken) => Ok(await listAdminUserService.ListAsync(query, cancellationToken));
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.AdminUsersRead)]
    public async Task<ActionResult<AdminUserResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getAdminUserService.GetAsync(id, cancellationToken));
    [HttpPost]
    [Authorize(Policy = PermissionCodes.AdminUsersWrite)]
    public async Task<ActionResult<AdminUserResponse>> Create(CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        AdminUserResponse response = await createAdminUserService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.AdminUsersWrite)]
    public async Task<ActionResult<AdminUserResponse>> Update(Guid id, UpdateAdminUserRequest request, CancellationToken cancellationToken) => Ok(await updateAdminUserService.UpdateAsync(id, request, cancellationToken));
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = PermissionCodes.AdminUsersWrite)]
    public Task Activate(Guid id, CancellationToken cancellationToken) => setStatusAdminUserService.SetStatusAsync(id, UserStatus.Active, cancellationToken);
    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = PermissionCodes.AdminUsersWrite)]
    public Task Suspend(Guid id, CancellationToken cancellationToken) => setStatusAdminUserService.SetStatusAsync(id, UserStatus.Suspended, cancellationToken);
    [HttpPost("{id:guid}/disable")]
    [Authorize(Policy = PermissionCodes.AdminUsersWrite)]
    public Task Disable(Guid id, CancellationToken cancellationToken) => setStatusAdminUserService.SetStatusAsync(id, UserStatus.Disabled, cancellationToken);
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCodes.AdminUsersWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteAdminUserService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Policy = PermissionCodes.AdminUsersWrite)]
    public async Task<IActionResult> ResetPassword(Guid id, ResetAdminPasswordRequest request, CancellationToken cancellationToken)
    {
        await resetPasswordAdminUserService.ResetPasswordAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/roles")]
    [Authorize(Policy = PermissionCodes.AdminUsersWrite)]
    public async Task<IActionResult> SetRoles(Guid id, AssignAdminRolesRequest request, CancellationToken cancellationToken)
    {
        await setRolesAdminUserService.SetRolesAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/permissions")]
    [Authorize(Policy = PermissionCodes.AdminUsersRead)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetPermissions(Guid id, CancellationToken cancellationToken) => Ok(await getPermissionsAdminUserService.GetPermissionsAsync(id, cancellationToken));
}
