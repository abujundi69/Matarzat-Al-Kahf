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
    private static bool IsEligible(Coupon coupon, CartItem item)
    {
        CouponProduct? product = coupon.Products.FirstOrDefault(mapping => mapping.ProductId == item.ProductVariant.ProductId);
        if (product?.IsExcluded == true)
        {
            return false;
        }

        bool scoped = coupon.Categories.Count > 0 || coupon.Products.Any(mapping => !mapping.IsExcluded);
        return !scoped || product is { IsExcluded: false } || coupon.Categories.Any(mapping => mapping.CategoryId == item.ProductVariant.Product.CategoryId);
    }
}
