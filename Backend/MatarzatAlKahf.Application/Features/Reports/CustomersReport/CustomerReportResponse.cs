using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Reports.DTOs;
public sealed record CustomerReportResponse(int NewCustomerCount, int InactiveCustomerCount, PagedResult<CustomerReportItem> Customers);
