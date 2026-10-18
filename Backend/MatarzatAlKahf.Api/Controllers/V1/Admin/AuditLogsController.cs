using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.AuditLogs.DTOs;
using MatarzatAlKahf.Application.Features.AuditLogs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/audit-logs")]
[Authorize(Policy = PermissionCodes.AuditRead)]
[Tags("Audit Logs")]
public sealed class AuditLogsController(IListAuditLogQueryService listAuditLogQueryService, IGetAuditLogQueryService getAuditLogQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AuditLogResponse>>> List([FromQuery] AuditLogQuery query, CancellationToken cancellationToken) => Ok(await listAuditLogQueryService.ListAsync(query, cancellationToken));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuditLogResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getAuditLogQueryService.GetAsync(id, cancellationToken));
}
