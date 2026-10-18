using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Responses;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.AdminUsers.Interfaces;
public interface IUpdateAdminUserService
{
    Task<AdminUserResponse> UpdateAsync(Guid id, UpdateAdminUserRequest request, CancellationToken cancellationToken);
}
