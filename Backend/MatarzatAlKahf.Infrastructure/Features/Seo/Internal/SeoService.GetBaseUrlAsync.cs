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
    private async Task<string> GetBaseUrlAsync(CancellationToken cancellationToken)
    {
        string? baseUrl = await dbContext.SeoSettings.AsNoTracking().Where(item => !item.IsDeleted).OrderBy(item => item.CreatedAtUtc).Select(item => item.BaseUrl).FirstOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ServiceUnavailableException("SEO BaseUrl must be configured before generating the sitemap.");
        }

        return baseUrl.TrimEnd('/');
    }
}
