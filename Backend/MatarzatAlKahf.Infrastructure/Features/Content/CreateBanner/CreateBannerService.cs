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
    public async Task<AdminBannerResponse> CreateAsync(SaveBannerRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var banner = new Banner();
        Apply(banner, request);
        dbContext.Banners.Add(banner);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("banner.created", banner, cancellationToken);
        return MapAdmin(banner);
    }
}
