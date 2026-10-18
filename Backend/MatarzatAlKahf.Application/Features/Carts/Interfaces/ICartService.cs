using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Carts.Interfaces;
public interface ICartService : IGetCartService, IAddItemCartService, IUpdateItemCartService, IRemoveItemCartService, IClearCartService, IMergeCartService, IApplyCouponCartService, IRemoveCouponCartService
{
}
