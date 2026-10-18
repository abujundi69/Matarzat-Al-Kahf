using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Orders.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Customer;
[ApiController]
[Route("api/v1/customer/orders")]
[Authorize(Policy = AuthenticationPolicyNames.Customer)]
[Tags("Orders")]
public sealed class CustomerOrdersController(IListCustomerOrderService listCustomerOrderService, IGetCustomerOrderService getCustomerOrderService, IGetCustomerInvoiceHtmlOrderService getCustomerInvoiceHtmlOrderService, ICancelCustomerOrderService cancelCustomerOrderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderSummaryResponse>>> List(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) => Ok(await listCustomerOrderService.ListCustomerAsync(page, pageSize, cancellationToken));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDetailsResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getCustomerOrderService.GetCustomerAsync(id, cancellationToken));
    [HttpGet("{id:guid}/invoice")]
    [Produces("text/html")]
    public async Task<ContentResult> Invoice(Guid id, CancellationToken cancellationToken) => Content(await getCustomerInvoiceHtmlOrderService.GetCustomerInvoiceHtmlAsync(id, cancellationToken), "text/html; charset=utf-8");
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDetailsResponse>> Cancel(Guid id, CancelOrderRequest request, CancellationToken cancellationToken) => Ok(await cancelCustomerOrderService.CancelCustomerAsync(id, request, cancellationToken));
}
