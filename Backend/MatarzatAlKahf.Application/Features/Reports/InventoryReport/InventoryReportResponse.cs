using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Reports.DTOs;
public sealed record InventoryReportResponse(int TotalOnHand, int TotalReserved, PagedResult<InventoryReportItem> Items);
