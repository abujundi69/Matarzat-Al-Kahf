using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;
using MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[Route("api/v1/contact")]
[Tags("Contact Messages")]
public sealed class ContactController(ISubmitContactMessageService submitContactMessageService) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("Contact")]
    public async Task<IActionResult> Submit(SubmitContactMessageRequest request, CancellationToken cancellationToken)
    {
        await submitContactMessageService.SubmitAsync(request, cancellationToken);
        return Accepted();
    }
}
