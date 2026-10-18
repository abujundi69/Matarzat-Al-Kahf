using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Orders.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[Route("api/v1/orders")]
[Tags("Orders")]
public sealed class GuestOrdersController(ILookupGuestOrderService lookupGuestOrderService) : ControllerBase
{
    [HttpGet("lookup")]
    [EnableRateLimiting("GuestOrderLookup")]
    public async Task<ActionResult<OrderDetailsResponse>> Lookup(string orderNumber, string phoneNumber, CancellationToken cancellationToken) => Ok(await lookupGuestOrderService.LookupGuestAsync(orderNumber, phoneNumber, cancellationToken));
}
