using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Carts.Interfaces;
public interface IClearCartService
{
    Task ClearAsync(string? guestToken, CancellationToken cancellationToken);
}
