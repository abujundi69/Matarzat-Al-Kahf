using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Orders.Interfaces;
public interface IListCustomerOrderService
{
    Task<PagedResult<OrderSummaryResponse>> ListCustomerAsync(int page, int pageSize, CancellationToken cancellationToken);
}
