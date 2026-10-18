using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[AllowAnonymous]
[Route("api/v1/payment-methods")]
[Tags("Payments")]
public sealed class PaymentMethodsController(IGetPublicPaymentMethodService getPublicPaymentMethodService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PaymentMethodResponse>>> Get(CancellationToken cancellationToken) => Ok(await getPublicPaymentMethodService.GetPublicAsync(cancellationToken));
}
