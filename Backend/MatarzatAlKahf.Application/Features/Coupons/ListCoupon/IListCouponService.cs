using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Responses;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Coupons.Interfaces;
public interface IListCouponService
{
    Task<PagedResult<CouponResponse>> ListAsync(CouponQuery query, CancellationToken cancellationToken);
}
