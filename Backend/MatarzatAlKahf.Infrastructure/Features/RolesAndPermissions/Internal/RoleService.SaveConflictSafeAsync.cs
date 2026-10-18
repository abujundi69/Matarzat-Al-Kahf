using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Requests;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.Interfaces;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.RolesAndPermissions.Services;
public sealed partial class RoleService
{
    private async Task SaveConflictSafeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)when (DbUpdateExceptionTranslator.IsUniqueViolation(exception))
        {
            throw new ConflictException("A role with the same name already exists.", exception);
        }
    }
}
