using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Payments.DTOs;

public sealed record PaymentQuery(
    int Page = PaginationRules.DefaultPage,
    int PageSize = PaginationRules.DefaultPageSize,
    PaymentStatus? Status = null,
    PaymentMethod? Method = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null);
