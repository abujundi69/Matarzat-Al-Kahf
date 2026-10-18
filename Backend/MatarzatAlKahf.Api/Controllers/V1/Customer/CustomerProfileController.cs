using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Customers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Customer;
[ApiController]
[Route("api/v1/customer/profile")]
[Authorize(Policy = AuthenticationPolicyNames.Customer)]
[Tags("Customers")]
public sealed class CustomerProfileController(IGetCustomerProfileService getCustomerProfileService, IUpdateCustomerProfileService updateCustomerProfileService, IChangePasswordCustomerProfileService changePasswordCustomerProfileService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CustomerProfileResponse>> Get(CancellationToken cancellationToken) => Ok(await getCustomerProfileService.GetAsync(cancellationToken));
    [HttpPut]
    public async Task<ActionResult<CustomerProfileResponse>> Update(UpdateCustomerProfileRequest request, CancellationToken cancellationToken) => Ok(await updateCustomerProfileService.UpdateAsync(request, cancellationToken));
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangeCustomerPasswordRequest request, CancellationToken cancellationToken)
    {
        await changePasswordCustomerProfileService.ChangePasswordAsync(request, cancellationToken);
        return NoContent();
    }
}
