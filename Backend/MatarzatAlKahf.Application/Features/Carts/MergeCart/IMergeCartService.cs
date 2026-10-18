using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Carts.Interfaces;
public interface IMergeCartService
{
    Task<CartResponse> MergeAsync(MergeCartRequest request, CancellationToken cancellationToken);
}
