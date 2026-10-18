using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/payments")]
[Tags("Payments")]
public sealed class AdminPaymentsController(IListAdminPaymentService listAdminPaymentService, IGetAdminPaymentService getAdminPaymentService, IGetAdminHistoryPaymentService getAdminHistoryPaymentService, IVerifyPaymentService verifyPaymentService, IRejectPaymentService rejectPaymentService, IRefundPaymentService refundPaymentService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.PaymentsRead)]
    public async Task<ActionResult<PagedResult<PaymentResponse>>> List([FromQuery] PaymentQuery query, CancellationToken cancellationToken) => Ok(await listAdminPaymentService.ListAdminAsync(query, cancellationToken));
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.PaymentsRead)]
    public async Task<ActionResult<PaymentResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getAdminPaymentService.GetAdminAsync(id, cancellationToken));
    [HttpGet("orders/{orderId:guid}")]
    [Authorize(Policy = PermissionCodes.PaymentsRead)]
    public async Task<ActionResult<IReadOnlyList<PaymentResponse>>> History(Guid orderId, CancellationToken cancellationToken) => Ok(await getAdminHistoryPaymentService.GetAdminHistoryAsync(orderId, cancellationToken));
    [HttpPost("{id:guid}/verify")]
    [Authorize(Policy = PermissionCodes.PaymentsVerify)]
    public async Task<ActionResult<PaymentResponse>> Verify(Guid id, VerifyPaymentRequest request, CancellationToken cancellationToken) => Ok(await verifyPaymentService.VerifyAsync(id, request, cancellationToken));
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = PermissionCodes.PaymentsVerify)]
    public async Task<ActionResult<PaymentResponse>> Reject(Guid id, RejectPaymentRequest request, CancellationToken cancellationToken) => Ok(await rejectPaymentService.RejectAsync(id, request, cancellationToken));
    [HttpPost("{id:guid}/refund")]
    [Authorize(Policy = PermissionCodes.PaymentsWrite)]
    public async Task<ActionResult<PaymentResponse>> Refund(Guid id, RefundPaymentRequest request, CancellationToken cancellationToken) => Ok(await refundPaymentService.RefundAsync(id, request, cancellationToken));
}
