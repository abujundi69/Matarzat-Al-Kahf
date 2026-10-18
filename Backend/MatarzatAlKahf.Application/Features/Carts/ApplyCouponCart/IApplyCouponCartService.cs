using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Carts.Interfaces;
public interface IApplyCouponCartService
{
    Task<CartResponse> ApplyCouponAsync(string? guestToken, ApplyCartCouponRequest request, CancellationToken cancellationToken);
}
