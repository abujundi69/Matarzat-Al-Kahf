using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;
using MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/contact-messages")]
[Tags("Contact Messages")]
public sealed class ContactMessagesController(IListContactMessageService listContactMessageService, IGetContactMessageService getContactMessageService, ISetStatusContactMessageService setStatusContactMessageService, IReplyContactMessageService replyContactMessageService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.ContactRead)]
    public async Task<ActionResult<PagedResult<ContactMessageResponse>>> List([FromQuery] ContactMessageQuery query, CancellationToken cancellationToken) => Ok(await listContactMessageService.ListAsync(query, cancellationToken));
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.ContactRead)]
    public async Task<ActionResult<ContactMessageResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getContactMessageService.GetAsync(id, cancellationToken));
    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = PermissionCodes.ContactWrite)]
    public async Task<ActionResult<ContactMessageResponse>> Status(Guid id, UpdateContactStatusRequest request, CancellationToken cancellationToken) => Ok(await setStatusContactMessageService.SetStatusAsync(id, request, cancellationToken));
    [HttpPost("{id:guid}/reply")]
    [Authorize(Policy = PermissionCodes.ContactWrite)]
    public async Task<ActionResult<ContactMessageResponse>> Reply(Guid id, ReplyContactMessageRequest request, CancellationToken cancellationToken) => Ok(await replyContactMessageService.ReplyAsync(id, request, cancellationToken));
}
