using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Reports.DTOs;
public sealed record ReportQuery(DateTime FromUtc, DateTime ToUtc, int Page = 1, int PageSize = 20);
