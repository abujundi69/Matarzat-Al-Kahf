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
    public async Task<string> RobotsAsync(CancellationToken cancellationToken)
    {
        SeoSetting? setting = await dbContext.SeoSettings.AsNoTracking().Where(item => !item.IsDeleted).OrderBy(item => item.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        var output = new StringBuilder().AppendLine("User-agent: *").AppendLine("Allow: /").AppendLine("Disallow: /api/").AppendLine("Disallow: /admin/");
        if (setting is not null)
        {
            if (!string.IsNullOrWhiteSpace(setting.RobotsAdditionalRules))
            {
                output.AppendLine(setting.RobotsAdditionalRules);
            }

            // A sitemap reference must be an absolute URL, and the sitemap itself is unavailable until
            // a base URL is configured, so crawlers are only pointed at it once there is one.
            if (!string.IsNullOrWhiteSpace(setting.BaseUrl))
            {
                output.Append("Sitemap: ").Append(setting.BaseUrl.TrimEnd('/')).AppendLine("/sitemap.xml");
            }
        }

        return output.ToString();
    }
}
