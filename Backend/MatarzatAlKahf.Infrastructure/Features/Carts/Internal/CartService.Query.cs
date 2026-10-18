using System.Security.Cryptography;
using System.Text;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Carts.Interfaces;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Carts.Services;
public sealed partial class CartService
{
    private IQueryable<Cart> Query() => dbContext.Carts.Include(cart => cart.Coupon).Include(cart => cart.Items).ThenInclude(item => item.ProductVariant).ThenInclude(variant => variant.InventoryItem).Include(cart => cart.Items).ThenInclude(item => item.ProductVariant).ThenInclude(variant => variant.Product).ThenInclude(product => product.Translations);
}
