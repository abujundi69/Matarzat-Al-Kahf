using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Banners.Interfaces;
public interface IUpdateBannerService
{
    Task<AdminBannerResponse> UpdateAsync(Guid id, SaveBannerRequest request, CancellationToken cancellationToken);
}
