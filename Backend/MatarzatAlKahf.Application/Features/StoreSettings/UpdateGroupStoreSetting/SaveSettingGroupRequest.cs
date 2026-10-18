using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.StoreSettings.DTOs;
public sealed record SaveSettingGroupRequest(IReadOnlyList<SaveSettingRequest> Settings);
