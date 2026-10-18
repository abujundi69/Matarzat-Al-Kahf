using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Banners.Interfaces;
public interface ICreateBannerService
{
    Task<AdminBannerResponse> CreateAsync(SaveBannerRequest request, CancellationToken cancellationToken);
}
