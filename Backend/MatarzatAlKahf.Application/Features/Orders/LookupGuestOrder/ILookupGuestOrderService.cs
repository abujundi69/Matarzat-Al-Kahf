using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Orders.Interfaces;
public interface ILookupGuestOrderService
{
    Task<OrderDetailsResponse> LookupGuestAsync(string orderNumber, string phoneNumber, CancellationToken cancellationToken);
}
