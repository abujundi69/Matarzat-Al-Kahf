using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Coupons.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/coupons")]
[Tags("Coupons")]
public sealed class CouponsController(IListCouponService listCouponService, IGetCouponService getCouponService, ICreateCouponService createCouponService, IUpdateCouponService updateCouponService, IDeleteCouponService deleteCouponService, ISetStatusCouponService setStatusCouponService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.CouponsRead)]
    public async Task<ActionResult<PagedResult<CouponResponse>>> List([FromQuery] CouponQuery query, CancellationToken cancellationToken) => Ok(await listCouponService.ListAsync(query, cancellationToken));
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.CouponsRead)]
    public async Task<ActionResult<CouponResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getCouponService.GetAsync(id, cancellationToken));
    [HttpPost]
    [Authorize(Policy = PermissionCodes.CouponsWrite)]
    public async Task<ActionResult<CouponResponse>> Create(SaveCouponRequest request, CancellationToken cancellationToken)
    {
        CouponResponse result = await createCouponService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.CouponsWrite)]
    public async Task<ActionResult<CouponResponse>> Update(Guid id, SaveCouponRequest request, CancellationToken cancellationToken) => Ok(await updateCouponService.UpdateAsync(id, request, cancellationToken));
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCodes.CouponsWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteCouponService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = PermissionCodes.CouponsWrite)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await setStatusCouponService.SetStatusAsync(id, CouponStatus.Active, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/disable")]
    [Authorize(Policy = PermissionCodes.CouponsWrite)]
    public async Task<IActionResult> Disable(Guid id, CancellationToken cancellationToken)
    {
        await setStatusCouponService.SetStatusAsync(id, CouponStatus.Disabled, cancellationToken);
        return NoContent();
    }
}
