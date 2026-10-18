using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Responses;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Coupons.Interfaces;
public interface ICouponService : IListCouponService, IGetCouponService, ICreateCouponService, IUpdateCouponService, IDeleteCouponService, ISetStatusCouponService, IValidateCouponService
{
}
