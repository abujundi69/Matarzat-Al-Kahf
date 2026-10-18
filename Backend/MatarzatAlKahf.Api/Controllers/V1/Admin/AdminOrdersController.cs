using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Orders.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/orders")]
[Tags("Orders")]
public sealed class AdminOrdersController(IListAdminOrderService listAdminOrderService, IGetAdminOrderService getAdminOrderService, IChangeStatusOrderService changeStatusOrderService, IUpdateNotesOrderService updateNotesOrderService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrdersRead)]
    public async Task<ActionResult<PagedResult<OrderSummaryResponse>>> List([FromQuery] OrderQuery query, CancellationToken cancellationToken) => Ok(await listAdminOrderService.ListAdminAsync(query, cancellationToken));
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.OrdersRead)]
    public async Task<ActionResult<OrderDetailsResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getAdminOrderService.GetAdminAsync(id, cancellationToken));
    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = PermissionCodes.OrdersStatusWrite)]
    public async Task<ActionResult<OrderDetailsResponse>> ChangeStatus(Guid id, ChangeOrderStatusRequest request, CancellationToken cancellationToken) => Ok(await changeStatusOrderService.ChangeStatusAsync(id, request, cancellationToken));
    [HttpPut("{id:guid}/notes")]
    [Authorize(Policy = PermissionCodes.OrdersWrite)]
    public async Task<ActionResult<OrderDetailsResponse>> UpdateNotes(Guid id, UpdateOrderNotesRequest request, CancellationToken cancellationToken) => Ok(await updateNotesOrderService.UpdateNotesAsync(id, request, cancellationToken));
}
