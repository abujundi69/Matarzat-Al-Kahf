using MatarzatAlKahf.Application.Features.StoreSettings.DTOs;
using MatarzatAlKahf.Application.Features.StoreSettings.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[Route("api/v1/settings")]
[Tags("Store Settings")]
public sealed class StoreSettingsController(IGetPublicGroupStoreSettingService getPublicGroupStoreSettingService) : ControllerBase
{
    [HttpGet("{group}")]
    public async Task<ActionResult<IReadOnlyList<StoreSettingResponse>>> Get(string group, CancellationToken cancellationToken) => Ok(await getPublicGroupStoreSettingService.GetPublicGroupAsync(group, cancellationToken));
}
