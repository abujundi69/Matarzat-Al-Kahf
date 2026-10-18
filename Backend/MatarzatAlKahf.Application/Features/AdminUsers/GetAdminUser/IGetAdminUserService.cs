using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Responses;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.AdminUsers.Interfaces;
public interface IGetAdminUserService
{
    Task<AdminUserResponse> GetAsync(Guid id, CancellationToken cancellationToken);
}
