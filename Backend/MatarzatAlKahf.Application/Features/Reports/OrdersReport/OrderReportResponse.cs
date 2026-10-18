using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Reports.DTOs;
public sealed record OrderReportResponse(int NewCount, int CompletedCount, int CancelledCount, int ReturnedCount, IReadOnlyList<NamedMetricResponse> ByCity, IReadOnlyList<NamedMetricResponse> ByPaymentMethod);
