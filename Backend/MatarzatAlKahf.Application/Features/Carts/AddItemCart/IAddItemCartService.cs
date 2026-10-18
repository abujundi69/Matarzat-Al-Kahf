using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Carts.Interfaces;
public interface IAddItemCartService
{
    Task<CartResponse> AddItemAsync(string? guestToken, AddCartItemRequest request, CancellationToken cancellationToken);
}
