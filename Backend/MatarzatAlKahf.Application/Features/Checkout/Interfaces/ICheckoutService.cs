using MatarzatAlKahf.Application.Features.Checkout.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Checkout.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Checkout.Interfaces;
public interface ICheckoutService : IPreviewCheckoutService, IPlaceOrderCheckoutService
{
}
