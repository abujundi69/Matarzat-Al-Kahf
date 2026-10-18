using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Dashboard.DTOs;
public sealed record DashboardProductResponse(Guid ProductId, string Sku, string Name, int QuantitySold, decimal Sales);
