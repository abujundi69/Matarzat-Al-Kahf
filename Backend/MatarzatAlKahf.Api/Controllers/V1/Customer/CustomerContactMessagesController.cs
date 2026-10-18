using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;
using MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Customer;
[ApiController]
[Route("api/v1/customer/messages")]
[Authorize(Policy = AuthenticationPolicyNames.Customer)]
[Tags("Contact Messages")]
public sealed class CustomerContactMessagesController(IListCustomerContactMessageService listCustomerContactMessageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ContactMessageResponse>>> List(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) => Ok(await listCustomerContactMessageService.ListCustomerAsync(page, pageSize, cancellationToken));
}
