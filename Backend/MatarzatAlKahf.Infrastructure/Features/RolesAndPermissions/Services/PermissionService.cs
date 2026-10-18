using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.Interfaces;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.RolesAndPermissions.Services;

public sealed class PermissionService(
    MatarzatAlKahfDbContext dbContext) : IPermissionService
{
    public async Task<IReadOnlyList<PermissionResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Permissions
            .AsNoTracking()
            .Where(permission => !permission.IsDeleted)
            .OrderBy(permission => permission.Code)
            .Select(permission => new PermissionResponse(
                permission.Id,
                permission.Code,
                permission.Name,
                permission.Description))
            .ToListAsync(cancellationToken);
    }
}
