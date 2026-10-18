using System.Linq.Expressions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.AuditLogs.DTOs;
using MatarzatAlKahf.Application.Features.AuditLogs.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.AuditLogs.Services;
public sealed partial class AuditLogQueryService
{
    public async Task<AuditLogResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        AuditLogResponse? log = await dbContext.AuditLogs.AsNoTracking().Where(item => item.Id == id && !item.IsDeleted).Select(Projection).SingleOrDefaultAsync(cancellationToken);
        return log ?? throw new NotFoundException("Audit log was not found.");
    }
}
