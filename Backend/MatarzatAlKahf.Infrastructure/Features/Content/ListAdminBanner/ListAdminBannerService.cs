using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Application.Features.Banners.Interfaces;
using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;
public sealed partial class BannerService
{
    public async Task<IReadOnlyList<AdminBannerResponse>> ListAdminAsync(CancellationToken cancellationToken)
    {
        List<Banner> banners = await dbContext.Banners.AsNoTracking().Include(banner => banner.Translations).Where(banner => !banner.IsDeleted).OrderBy(banner => banner.Position).ThenBy(banner => banner.SortOrder).ToListAsync(cancellationToken);
        return banners.Select(MapAdmin).ToArray();
    }
}
