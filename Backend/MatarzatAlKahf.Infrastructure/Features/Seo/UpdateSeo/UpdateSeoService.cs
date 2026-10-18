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
    public async Task<SeoSettingsResponse> UpdateAsync(SaveSeoSettingsRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        SeoSetting? setting = await dbContext.SeoSettings.Where(item => !item.IsDeleted).OrderBy(item => item.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        if (setting is null)
        {
            setting = new SeoSetting();
            dbContext.SeoSettings.Add(setting);
        }

        string[] previousImageUrls =
        [
            setting.ArabicOpenGraphImageUrl,
            setting.EnglishOpenGraphImageUrl
        ];

        setting.BaseUrl = request.BaseUrl.TrimEnd('/');
        setting.ArabicTitle = request.ArabicTitle.Trim();
        setting.EnglishTitle = request.EnglishTitle.Trim();
        setting.ArabicDescription = request.ArabicDescription.Trim();
        setting.EnglishDescription = request.EnglishDescription.Trim();
        setting.ArabicOpenGraphImageUrl = request.ArabicOpenGraphImageUrl?.Trim() ?? string.Empty;
        setting.EnglishOpenGraphImageUrl = request.EnglishOpenGraphImageUrl?.Trim() ?? string.Empty;
        setting.RobotsAdditionalRules = request.RobotsAdditionalRules?.Trim() ?? string.Empty;
        setting.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (string previousImageUrl in previousImageUrls.Distinct(
                     StringComparer.OrdinalIgnoreCase))
        {
            await fileCleanup.DeleteIfUnreferencedAsync(
                previousImageUrl,
                cancellationToken);
        }

        await auditService.RecordAsync("seo.settings.updated", nameof(SeoSetting), setting.Id, "Default SEO settings were updated.", cancellationToken);
        return Map(setting);
    }
}
