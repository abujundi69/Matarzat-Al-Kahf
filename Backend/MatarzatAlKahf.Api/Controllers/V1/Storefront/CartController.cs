using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Carts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[Route("api/v1/cart")]
[Tags("Cart")]
public sealed class CartController(IGetCartService getCartService, IAddItemCartService addItemCartService, IUpdateItemCartService updateItemCartService, IRemoveItemCartService removeItemCartService, IClearCartService clearCartService, IMergeCartService mergeCartService, IApplyCouponCartService applyCouponCartService, IRemoveCouponCartService removeCouponCartService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<CartResponse>> Get([FromHeader(Name = "X-Cart-Token")] string? cartToken, CancellationToken cancellationToken) => Ok(await getCartService.GetAsync(cartToken, cancellationToken));
    [HttpPost("items")]
    [AllowAnonymous]
    public async Task<ActionResult<CartResponse>> AddItem(AddCartItemRequest request, [FromHeader(Name = "X-Cart-Token")] string? cartToken, CancellationToken cancellationToken) => Ok(await addItemCartService.AddItemAsync(cartToken, request, cancellationToken));
    [HttpPut("items/{itemId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CartResponse>> UpdateItem(Guid itemId, UpdateCartItemRequest request, [FromHeader(Name = "X-Cart-Token")] string? cartToken, CancellationToken cancellationToken) => Ok(await updateItemCartService.UpdateItemAsync(cartToken, itemId, request, cancellationToken));
    [HttpDelete("items/{itemId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CartResponse>> RemoveItem(Guid itemId, [FromHeader(Name = "X-Cart-Token")] string? cartToken, CancellationToken cancellationToken) => Ok(await removeItemCartService.RemoveItemAsync(cartToken, itemId, cancellationToken));
    [HttpDelete]
    [AllowAnonymous]
    public async Task<IActionResult> Clear([FromHeader(Name = "X-Cart-Token")] string? cartToken, CancellationToken cancellationToken)
    {
        await clearCartService.ClearAsync(cartToken, cancellationToken);
        return NoContent();
    }

    [HttpPost("merge")]
    [Authorize(Policy = AuthenticationPolicyNames.Customer)]
    public async Task<ActionResult<CartResponse>> Merge(MergeCartRequest request, CancellationToken cancellationToken) => Ok(await mergeCartService.MergeAsync(request, cancellationToken));
    [HttpPost("coupon")]
    [AllowAnonymous]
    public async Task<ActionResult<CartResponse>> ApplyCoupon(ApplyCartCouponRequest request, [FromHeader(Name = "X-Cart-Token")] string? cartToken, CancellationToken cancellationToken) => Ok(await applyCouponCartService.ApplyCouponAsync(cartToken, request, cancellationToken));
    [HttpDelete("coupon")]
    [AllowAnonymous]
    public async Task<ActionResult<CartResponse>> RemoveCoupon([FromHeader(Name = "X-Cart-Token")] string? cartToken, CancellationToken cancellationToken) => Ok(await removeCouponCartService.RemoveCouponAsync(cartToken, cancellationToken));
}
