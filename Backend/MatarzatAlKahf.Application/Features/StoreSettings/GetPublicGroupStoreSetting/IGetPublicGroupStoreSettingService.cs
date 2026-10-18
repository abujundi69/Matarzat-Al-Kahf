using MatarzatAlKahf.Application.Features.StoreSettings.DTOs;

namespace MatarzatAlKahf.Application.Features.StoreSettings.Interfaces;
public interface IGetPublicGroupStoreSettingService
{
    Task<IReadOnlyList<StoreSettingResponse>> GetPublicGroupAsync(string group, CancellationToken cancellationToken);
}
