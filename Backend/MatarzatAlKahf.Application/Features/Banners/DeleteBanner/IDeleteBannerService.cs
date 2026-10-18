using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Banners.Interfaces;
public interface IDeleteBannerService
{
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
