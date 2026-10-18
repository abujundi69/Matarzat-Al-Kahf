using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.Interfaces;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/customers")]
[Tags("Customers")]
public sealed class AdminCustomersController(IListAdminCustomerService listAdminCustomerService, IGetAdminCustomerService getAdminCustomerService, IUpdateAdminCustomerService updateAdminCustomerService, ISetStatusAdminCustomerService setStatusAdminCustomerService, IGetAddressesAdminCustomerService getAddressesAdminCustomerService, IGetOrdersAdminCustomerService getOrdersAdminCustomerService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.CustomersRead)]
    public async Task<ActionResult<PagedResult<AdminCustomerResponse>>> List([FromQuery] AdminCustomerQuery query, CancellationToken cancellationToken) => Ok(await listAdminCustomerService.ListAsync(query, cancellationToken));
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.CustomersRead)]
    public async Task<ActionResult<AdminCustomerResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getAdminCustomerService.GetAsync(id, cancellationToken));
    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.CustomersWrite)]
    public async Task<ActionResult<AdminCustomerResponse>> Update(Guid id, UpdateAdminCustomerRequest request, CancellationToken cancellationToken) => Ok(await updateAdminCustomerService.UpdateAsync(id, request, cancellationToken));
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = PermissionCodes.CustomersStatusWrite)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await setStatusAdminCustomerService.SetStatusAsync(id, UserStatus.Active, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = PermissionCodes.CustomersStatusWrite)]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
    {
        await setStatusAdminCustomerService.SetStatusAsync(id, UserStatus.Suspended, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/disable")]
    [Authorize(Policy = PermissionCodes.CustomersStatusWrite)]
    public async Task<IActionResult> Disable(Guid id, CancellationToken cancellationToken)
    {
        await setStatusAdminCustomerService.SetStatusAsync(id, UserStatus.Disabled, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/addresses")]
    [Authorize(Policy = PermissionCodes.CustomersRead)]
    public async Task<ActionResult<IReadOnlyList<CustomerAddressResponse>>> Addresses(Guid id, CancellationToken cancellationToken) => Ok(await getAddressesAdminCustomerService.GetAddressesAsync(id, cancellationToken));
    [HttpGet("{id:guid}/orders")]
    [Authorize(Policy = PermissionCodes.CustomersRead)]
    public async Task<ActionResult<PagedResult<OrderSummaryResponse>>> Orders(Guid id, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) => Ok(await getOrdersAdminCustomerService.GetOrdersAsync(id, page, pageSize, cancellationToken));
}
