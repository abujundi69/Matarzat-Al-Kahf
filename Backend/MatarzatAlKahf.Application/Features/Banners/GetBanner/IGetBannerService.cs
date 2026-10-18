using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Banners.Interfaces;
public interface IGetBannerService
{
    Task<AdminBannerResponse> GetAsync(Guid id, CancellationToken cancellationToken);
}
