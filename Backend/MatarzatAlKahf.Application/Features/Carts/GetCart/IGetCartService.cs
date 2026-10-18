using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Carts.Interfaces;
public interface IGetCartService
{
    Task<CartResponse> GetAsync(string? guestToken, CancellationToken cancellationToken);
}
