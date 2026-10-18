using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MatarzatAlKahf.Api.Controllers.V1;

[ApiController]
[Route("api/v1/health")]
[AllowAnonymous]
[Tags("Health")]
public sealed class HealthController(HealthCheckService healthChecks) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        HealthReport report = await healthChecks.CheckHealthAsync(
            registration => registration.Tags.Contains("ready"),
            cancellationToken);
        bool databaseConnected = report.Status == HealthStatus.Healthy;

        var response = new
        {
            status = databaseConnected ? "Healthy" : "Unhealthy",
            service = "MatarzatAlKahf.Api",
            database = databaseConnected ? "Connected" : "Disconnected",
            utcTime = DateTime.UtcNow
        };

        return databaseConnected
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}
