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
    public async Task<CouponResponse> UpdateAsync(Guid id, SaveCouponRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        Coupon coupon = await FindAsync(id, true, cancellationToken);
        dbContext.CouponCategories.RemoveRange(coupon.Categories);
        dbContext.CouponProducts.RemoveRange(coupon.Products);
        coupon.Categories.Clear();
        coupon.Products.Clear();
        await ApplyAsync(coupon, request, cancellationToken);
        coupon.UpdatedAtUtc = DateTime.UtcNow;
        await SaveSafeAsync(cancellationToken);
        await AuditAsync("coupon.updated", coupon, cancellationToken);
        return await GetAsync(id, cancellationToken);
    }
}
