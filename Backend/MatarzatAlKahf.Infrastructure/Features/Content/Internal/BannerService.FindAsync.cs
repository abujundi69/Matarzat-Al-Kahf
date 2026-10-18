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
    private async Task<Banner> FindAsync(Guid id, bool tracked, CancellationToken cancellationToken)
    {
        IQueryable<Banner> query = dbContext.Banners.Include(banner => banner.Translations).Where(banner => banner.Id == id && !banner.IsDeleted);
        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Banner was not found.");
    }
}
