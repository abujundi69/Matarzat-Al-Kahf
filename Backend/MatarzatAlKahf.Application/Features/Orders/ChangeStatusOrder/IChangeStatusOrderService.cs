using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Orders.Interfaces;
public interface IChangeStatusOrderService
{
    Task<OrderDetailsResponse> ChangeStatusAsync(Guid id, ChangeOrderStatusRequest request, CancellationToken cancellationToken);
}
