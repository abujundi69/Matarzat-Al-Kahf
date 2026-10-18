using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.StoreSettings.DTOs;
using MatarzatAlKahf.Application.Features.StoreSettings.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/settings")]
[Tags("Store Settings")]
public sealed class AdminStoreSettingsController(
    IGetAdminGroupStoreSettingService getAdminGroupStoreSettingService,
    IUpdateGroupStoreSettingService updateGroupStoreSettingService,
    IAuthorizationService authorizationService) : ControllerBase
{
    private const string PaymentMethodsGroup = "payment-methods";

    [HttpGet("{group}")]
    [Authorize(Policy = PermissionCodes.SettingsRead)]
    public async Task<ActionResult<IReadOnlyList<StoreSettingResponse>>> Get(string group, CancellationToken cancellationToken)
    {
        if (IsPaymentMethodsGroup(group) &&
            !await IsAuthorizedAsync(PermissionCodes.PaymentsRead))
        {
            return Forbid();
        }

        return Ok(await getAdminGroupStoreSettingService.GetAdminGroupAsync(group, cancellationToken));
    }

    [HttpPut("{group}")]
    [Authorize(Policy = PermissionCodes.SettingsWrite)]
    public async Task<ActionResult<IReadOnlyList<StoreSettingResponse>>> Update(string group, SaveSettingGroupRequest request, CancellationToken cancellationToken)
    {
        if (IsPaymentMethodsGroup(group) &&
            !await IsAuthorizedAsync(PermissionCodes.PaymentsWrite))
        {
            return Forbid();
        }

        return Ok(await updateGroupStoreSettingService.UpdateGroupAsync(group, request, cancellationToken));
    }

    private static bool IsPaymentMethodsGroup(string group) =>
        string.Equals(group?.Trim(), PaymentMethodsGroup, StringComparison.OrdinalIgnoreCase);

    private async Task<bool> IsAuthorizedAsync(string policy) =>
        (await authorizationService.AuthorizeAsync(User, policy)).Succeeded;
}
