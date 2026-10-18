using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Carts.Interfaces;
public interface IRemoveCouponCartService
{
    Task<CartResponse> RemoveCouponAsync(string? guestToken, CancellationToken cancellationToken);
}
