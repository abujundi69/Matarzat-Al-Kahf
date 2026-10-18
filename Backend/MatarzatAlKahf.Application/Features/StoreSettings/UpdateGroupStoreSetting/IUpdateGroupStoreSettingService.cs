using MatarzatAlKahf.Application.Features.StoreSettings.DTOs;

namespace MatarzatAlKahf.Application.Features.StoreSettings.Interfaces;
public interface IUpdateGroupStoreSettingService
{
    Task<IReadOnlyList<StoreSettingResponse>> UpdateGroupAsync(string group, SaveSettingGroupRequest request, CancellationToken cancellationToken);
}
