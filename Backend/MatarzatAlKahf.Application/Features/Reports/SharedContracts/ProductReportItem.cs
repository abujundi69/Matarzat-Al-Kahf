using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Reports.DTOs;
public sealed record ProductReportItem(Guid ProductId, string Name, string Sku, int QuantitySold, decimal Sales, int AvailableQuantity);
