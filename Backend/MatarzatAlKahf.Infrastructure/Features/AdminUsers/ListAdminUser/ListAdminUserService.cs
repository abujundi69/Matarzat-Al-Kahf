using System.Net.Mail;
using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Responses;
using MatarzatAlKahf.Application.Features.AdminUsers.Interfaces;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.AdminUsers.Services;
public sealed partial class AdminUserService
{
    public async Task<PagedResult<AdminUserResponse>> ListAsync(AdminUserQuery query, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(query.Page, query.PageSize);
        IQueryable<AdminUser> users = dbContext.AdminUsers.AsNoTracking().Where(user => !user.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();
            users = users.Where(user => user.FullName.Contains(search) || user.PhoneNumber.Contains(search) || (user.Email != null && user.Email.Contains(search)));
        }

        if (query.Status.HasValue)
        {
            users = users.Where(user => user.Status == query.Status);
        }

        if (query.RoleId.HasValue)
        {
            users = users.Where(user => user.UserRoles.Any(mapping => mapping.RoleId == query.RoleId));
        }

        int total = await users.CountAsync(cancellationToken);
        List<AdminUser> pageUsers = await users.OrderBy(user => user.FullName).ThenBy(user => user.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).Include(user => user.UserRoles).ThenInclude(mapping => mapping.Role).ThenInclude(role => role.RolePermissions).ThenInclude(mapping => mapping.Permission).ToListAsync(cancellationToken);
        AdminUserResponse[] items = pageUsers.Select(Map).ToArray();
        return PagedResultFactory.Create(items, query.Page, query.PageSize, total);
    }
}
