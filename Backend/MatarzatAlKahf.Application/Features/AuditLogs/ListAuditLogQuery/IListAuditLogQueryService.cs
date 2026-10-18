using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.AuditLogs.DTOs;

namespace MatarzatAlKahf.Application.Features.AuditLogs.Interfaces;
public interface IListAuditLogQueryService
{
    Task<PagedResult<AuditLogResponse>> ListAsync(AuditLogQuery query, CancellationToken cancellationToken);
}
