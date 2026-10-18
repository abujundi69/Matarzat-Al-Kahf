using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Dashboard.DTOs;
using MatarzatAlKahf.Application.Features.Dashboard.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/dashboard")]
[Authorize(Policy = PermissionCodes.ReportsSalesRead)]
[Tags("Dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get(
        CancellationToken cancellationToken) =>
        Ok(await dashboardService.GetAsync(cancellationToken));
}
