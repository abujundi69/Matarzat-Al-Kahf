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
    private static decimal CalculateDiscount(Coupon coupon, decimal eligible)
    {
        decimal discount = coupon.DiscountType == DiscountType.Percentage ? eligible * coupon.DiscountValue / 100 : Math.Min(eligible, coupon.DiscountValue);
        if (coupon.MaximumDiscountAmount.HasValue)
        {
            discount = Math.Min(discount, coupon.MaximumDiscountAmount.Value);
        }

        return decimal.Round(discount, 2);
    }
}
