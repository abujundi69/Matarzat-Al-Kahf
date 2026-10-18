using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Orders.Interfaces;
public interface ICancelCustomerOrderService
{
    Task<OrderDetailsResponse> CancelCustomerAsync(Guid id, CancelOrderRequest request, CancellationToken cancellationToken);
}
