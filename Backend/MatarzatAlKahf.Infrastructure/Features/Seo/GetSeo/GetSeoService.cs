using System.Text;
using System.Xml;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.Seo.DTOs;
using MatarzatAlKahf.Application.Features.Seo.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Seo.Services;
public sealed partial class SeoService
{
    public async Task<SeoSettingsResponse> GetAsync(CancellationToken cancellationToken)
    {
        SeoSetting? setting = await dbContext.SeoSettings.AsNoTracking().Where(item => !item.IsDeleted).OrderBy(item => item.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        return setting is null ? new SeoSettingsResponse(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, null, null) : Map(setting);
    }
}
