using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Delivery.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Delivery.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Delivery.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/delivery/locations")]
[Tags("Delivery")]
public sealed class AdminDeliveryLocationsController(IListAdminDeliveryLocationService listAdminDeliveryLocationService, IGetDeliveryLocationService getDeliveryLocationService, ICreateDeliveryLocationService createDeliveryLocationService, IUpdateDeliveryLocationService updateDeliveryLocationService, IDeleteDeliveryLocationService deleteDeliveryLocationService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.DeliveryRead)]
    public async Task<ActionResult<IReadOnlyList<AdminDeliveryLocationResponse>>> List(CancellationToken cancellationToken) => Ok(await listAdminDeliveryLocationService.ListAdminAsync(cancellationToken));
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.DeliveryRead)]
    public async Task<ActionResult<AdminDeliveryLocationResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getDeliveryLocationService.GetAsync(id, cancellationToken));
    [HttpPost]
    [Authorize(Policy = PermissionCodes.DeliveryWrite)]
    public async Task<ActionResult<AdminDeliveryLocationResponse>> Create(SaveDeliveryLocationRequest request, CancellationToken cancellationToken)
    {
        AdminDeliveryLocationResponse response = await createDeliveryLocationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.DeliveryWrite)]
    public async Task<ActionResult<AdminDeliveryLocationResponse>> Update(Guid id, SaveDeliveryLocationRequest request, CancellationToken cancellationToken) => Ok(await updateDeliveryLocationService.UpdateAsync(id, request, cancellationToken));
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCodes.DeliveryWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteDeliveryLocationService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
