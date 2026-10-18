using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Orders.Interfaces;
public interface IListAdminOrderService
{
    Task<PagedResult<OrderSummaryResponse>> ListAdminAsync(OrderQuery query, CancellationToken cancellationToken);
}
