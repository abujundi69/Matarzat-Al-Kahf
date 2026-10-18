using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;
using MatarzatAlKahf.Application.Features.CustomerAddresses.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Customer;
[ApiController]
[Route("api/v1/customer/addresses")]
[Authorize(Policy = AuthenticationPolicyNames.Customer)]
[Tags("Customer Addresses")]
public sealed class CustomerAddressesController(IListCustomerAddressService listCustomerAddressService, IGetCustomerAddressService getCustomerAddressService, ICreateCustomerAddressService createCustomerAddressService, IUpdateCustomerAddressService updateCustomerAddressService, IDeleteCustomerAddressService deleteCustomerAddressService, ISetDefaultCustomerAddressService setDefaultCustomerAddressService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerAddressResponse>>> List(CancellationToken cancellationToken) => Ok(await listCustomerAddressService.ListAsync(cancellationToken));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerAddressResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getCustomerAddressService.GetAsync(id, cancellationToken));
    [HttpPost]
    public async Task<ActionResult<CustomerAddressResponse>> Create(SaveCustomerAddressRequest request, CancellationToken cancellationToken)
    {
        CustomerAddressResponse response = await createCustomerAddressService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerAddressResponse>> Update(Guid id, SaveCustomerAddressRequest request, CancellationToken cancellationToken) => Ok(await updateCustomerAddressService.UpdateAsync(id, request, cancellationToken));
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteCustomerAddressService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/set-default")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        await setDefaultCustomerAddressService.SetDefaultAsync(id, cancellationToken);
        return NoContent();
    }
}
