using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Carts.Interfaces;
public interface IUpdateItemCartService
{
    Task<CartResponse> UpdateItemAsync(string? guestToken, Guid itemId, UpdateCartItemRequest request, CancellationToken cancellationToken);
}
