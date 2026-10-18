using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.AdminUsers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/profile")]
[Authorize(Policy = AuthenticationPolicyNames.Admin)]
[Tags("Admin Users")]
public sealed class AdminProfileController(IGetProfileAdminUserService getProfileAdminUserService, IUpdateProfileAdminUserService updateProfileAdminUserService, IChangePasswordAdminUserService changePasswordAdminUserService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AdminProfileDto>> Get(CancellationToken cancellationToken) => Ok(await getProfileAdminUserService.GetProfileAsync(cancellationToken));
    [HttpPut]
    public async Task<ActionResult<AdminProfileDto>> Update(UpdateAdminUserRequest request, CancellationToken cancellationToken) => Ok(await updateProfileAdminUserService.UpdateProfileAsync(request, cancellationToken));
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangeAdminPasswordRequest request, CancellationToken cancellationToken)
    {
        await changePasswordAdminUserService.ChangePasswordAsync(request, cancellationToken);
        return NoContent();
    }
}
