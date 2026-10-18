using MatarzatAlKahf.Application.Features.Delivery.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Delivery.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[Route("api/v1/delivery/locations")]
[Tags("Delivery")]
public sealed class DeliveryLocationsController(IListPublicDeliveryLocationService listPublicDeliveryLocationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeliveryLocationResponse>>> List(CancellationToken cancellationToken) => Ok(await listPublicDeliveryLocationService.ListPublicAsync(cancellationToken));
}
