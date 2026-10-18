using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Reports.DTOs;
public sealed record DateSalesResponse(DateTime DateUtc, int OrderCount, decimal Sales);
