using MatarzatAlKahf.Application.Authentication.Unified;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MatarzatAlKahf.Api.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json", "application/problem+json")]
public sealed class AuthenticationController(
    IUnifiedAuthenticationService authenticationService) : ControllerBase
{
    /// <summary>
    /// Authenticates a customer or administrative user using a phone number and password.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("UnifiedLogin")]
    [ProducesResponseType<UnifiedLoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UnifiedLoginResponse>> Login(
        [FromBody] UnifiedLoginRequest request,
        CancellationToken cancellationToken)
    {
        UnifiedLoginResponse response = await authenticationService.LoginAsync(
            request,
            cancellationToken);
        return Ok(response);
    }
}
