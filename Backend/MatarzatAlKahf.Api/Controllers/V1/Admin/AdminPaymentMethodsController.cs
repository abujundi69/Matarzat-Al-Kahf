using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/payment-methods")]
[Tags("Payments")]
public sealed class AdminPaymentMethodsController(IGetAdminPaymentMethodService getAdminPaymentMethodService, IUpdatePaymentMethodService updatePaymentMethodService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.PaymentsRead)]
    public async Task<ActionResult<IReadOnlyList<PaymentMethodResponse>>> Get(CancellationToken cancellationToken) => Ok(await getAdminPaymentMethodService.GetAdminAsync(cancellationToken));
    [HttpPut("{method}")]
    [Authorize(Policy = PermissionCodes.PaymentsWrite)]
    public async Task<ActionResult<PaymentMethodResponse>> Update(PaymentMethod method, UpdatePaymentMethodRequest request, CancellationToken cancellationToken) => Ok(await updatePaymentMethodService.UpdateAsync(method, request, cancellationToken));
}
