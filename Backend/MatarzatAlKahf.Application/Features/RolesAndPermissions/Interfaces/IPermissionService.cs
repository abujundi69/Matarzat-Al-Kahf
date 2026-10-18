using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.RolesAndPermissions.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyList<PermissionResponse>> ListAsync(CancellationToken cancellationToken);
}
