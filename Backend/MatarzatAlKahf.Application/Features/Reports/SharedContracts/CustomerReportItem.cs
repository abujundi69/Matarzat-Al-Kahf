using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Reports.DTOs;
public sealed record CustomerReportItem(Guid CustomerId, string Name, string PhoneNumber, int OrderCount, decimal Spending, DateTime CreatedAtUtc, DateTime? LastLoginAtUtc);
