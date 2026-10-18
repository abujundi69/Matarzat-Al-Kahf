using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Reports.DTOs;
public sealed record ProductReportResponse(PagedResult<ProductReportItem> Products, int LowStockCount, int OutOfStockCount);
