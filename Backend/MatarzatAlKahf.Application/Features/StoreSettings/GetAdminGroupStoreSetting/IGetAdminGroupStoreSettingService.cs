using MatarzatAlKahf.Application.Features.StoreSettings.DTOs;

namespace MatarzatAlKahf.Application.Features.StoreSettings.Interfaces;
public interface IGetAdminGroupStoreSettingService
{
    Task<IReadOnlyList<StoreSettingResponse>> GetAdminGroupAsync(string group, CancellationToken cancellationToken);
}
