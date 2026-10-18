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
    private static CouponResponse Map(Coupon coupon) => new(coupon.Id, coupon.Code, coupon.DiscountType, coupon.DiscountValue, coupon.MinimumOrderAmount, coupon.MaximumDiscountAmount, coupon.StartsAtUtc, coupon.EndsAtUtc, coupon.TotalUsageLimit, coupon.Usages.Count, coupon.UsageLimitPerCustomer, coupon.Status, coupon.Categories.Select(item => item.CategoryId).ToArray(), coupon.Products.Where(item => !item.IsExcluded).Select(item => item.ProductId).ToArray(), coupon.Products.Where(item => item.IsExcluded).Select(item => item.ProductId).ToArray());
}
