using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Banners.Interfaces;
public interface IListPublicBannerService
{
    Task<IReadOnlyList<BannerResponse>> ListPublicAsync(BannerPosition? position, CancellationToken cancellationToken);
}
