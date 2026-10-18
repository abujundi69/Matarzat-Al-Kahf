using MatarzatAlKahf.Application.Features.StoreSettings.DTOs;

namespace MatarzatAlKahf.Application.Features.StoreSettings.Interfaces;
public interface IStoreSettingService : IGetPublicGroupStoreSettingService, IGetAdminGroupStoreSettingService, IUpdateGroupStoreSettingService
{
}
