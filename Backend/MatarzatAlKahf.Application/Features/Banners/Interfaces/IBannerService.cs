using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Banners.Interfaces;
public interface IBannerService : IListPublicBannerService, IListAdminBannerService, IGetBannerService, ICreateBannerService, IUpdateBannerService, IDeleteBannerService
{
}
