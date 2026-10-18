using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;

public sealed record AdminUserQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    UserStatus? Status = null,
    Guid? RoleId = null);
