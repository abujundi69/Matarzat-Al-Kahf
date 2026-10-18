using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Carts.Interfaces;
public interface IRemoveItemCartService
{
    Task<CartResponse> RemoveItemAsync(string? guestToken, Guid itemId, CancellationToken cancellationToken);
}
