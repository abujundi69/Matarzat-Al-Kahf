using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;

public sealed record AdminCustomerQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    UserStatus? Status = null,
    DateTime? RegisteredFromUtc = null,
    DateTime? RegisteredToUtc = null);
