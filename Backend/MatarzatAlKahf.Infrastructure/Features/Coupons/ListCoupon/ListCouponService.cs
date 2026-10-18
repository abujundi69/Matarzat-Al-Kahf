using System.Security.Cryptography;
using System.Text;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Coupons.Interfaces;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Coupons.Services;
public sealed partial class CouponService
{
    public async Task<PagedResult<CouponResponse>> ListAsync(CouponQuery query, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(query.Page, query.PageSize);
        IQueryable<Coupon> coupons = Query().Where(coupon => !coupon.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();
            coupons = coupons.Where(coupon => coupon.Code.Contains(search));
        }

        if (query.Status.HasValue)
        {
            coupons = coupons.Where(coupon => coupon.Status == query.Status);
        }

        int count = await coupons.CountAsync(cancellationToken);
        List<Coupon> page = await coupons.AsNoTracking().OrderBy(coupon => coupon.Code).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(page.Select(Map).ToArray(), query.Page, query.PageSize, count);
    }
}
