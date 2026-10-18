using System.Security.Claims;
using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/auth")]
[Authorize(Policy = AuthenticationPolicyNames.Admin)]
[Produces("application/json", "application/problem+json")]
[Tags("Admin Users")]
public sealed class AdminSessionsController(
    IAdminSessionService adminSessionService) : ControllerBase
{
    /// <summary>
    /// Revokes the administrative session behind the presented token so it stops being
    /// accepted immediately instead of staying valid until it expires.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        string tokenId = User.FindFirstValue(
            System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti) ?? string.Empty;
        await adminSessionService.LogoutAsync(tokenId, cancellationToken);
        return NoContent();
    }
}
