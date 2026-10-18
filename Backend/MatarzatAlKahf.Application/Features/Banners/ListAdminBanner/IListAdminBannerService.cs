using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Banners.Interfaces;
public interface IListAdminBannerService
{
    Task<IReadOnlyList<AdminBannerResponse>> ListAdminAsync(CancellationToken cancellationToken);
}
