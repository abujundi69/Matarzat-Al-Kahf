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
    public async Task<string> SitemapAsync(CancellationToken cancellationToken)
    {
        string baseUrl = await GetBaseUrlAsync(cancellationToken);
        var urls = new List<(string Location, DateTime? Modified)>();
        var products = await dbContext.ProductTranslations.AsNoTracking().Where(translation => !translation.IsDeleted && !translation.Product.IsDeleted && translation.Product.Status == ProductStatus.Active).Select(translation => new { translation.Language, translation.Slug, translation.Product.UpdatedAtUtc, translation.Product.CreatedAtUtc }).ToListAsync(cancellationToken);
        urls.AddRange(products.Select(item => ($"{baseUrl}/{LanguagePath(item.Language)}/products/{Uri.EscapeDataString(item.Slug)}", (DateTime? )(item.UpdatedAtUtc ?? item.CreatedAtUtc))));
        var categories = await dbContext.CategoryTranslations.AsNoTracking().Where(translation => !translation.IsDeleted && !translation.Category.IsDeleted && translation.Category.IsActive).Select(translation => new { translation.Language, translation.Slug, translation.Category.UpdatedAtUtc, translation.Category.CreatedAtUtc }).ToListAsync(cancellationToken);
        urls.AddRange(categories.Select(item => ($"{baseUrl}/{LanguagePath(item.Language)}/categories/{Uri.EscapeDataString(item.Slug)}", (DateTime? )(item.UpdatedAtUtc ?? item.CreatedAtUtc))));
        var pages = await dbContext.ContentPageTranslations.AsNoTracking().Where(translation => !translation.IsDeleted && !translation.ContentPage.IsDeleted && translation.ContentPage.IsPublished).Select(translation => new { translation.Language, translation.Slug, translation.ContentPage.UpdatedAtUtc, translation.ContentPage.CreatedAtUtc }).ToListAsync(cancellationToken);
        urls.AddRange(pages.Select(item => ($"{baseUrl}/{LanguagePath(item.Language)}/pages/{Uri.EscapeDataString(item.Slug)}", (DateTime? )(item.UpdatedAtUtc ?? item.CreatedAtUtc))));
        await using var output = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(false),
            Indent = true,
            Async = true
        };
        using XmlWriter writer = XmlWriter.Create(output, settings);
        await writer.WriteStartDocumentAsync();
        await writer.WriteStartElementAsync(null, "urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
        foreach ((string location, DateTime? modified)in urls)
        {
            await writer.WriteStartElementAsync(null, "url", null);
            await writer.WriteElementStringAsync(null, "loc", null, location);
            if (modified.HasValue)
            {
                await writer.WriteElementStringAsync(null, "lastmod", null, modified.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
            }

            await writer.WriteEndElementAsync();
        }

        await writer.WriteEndElementAsync();
        await writer.WriteEndDocumentAsync();
        await writer.FlushAsync();
        return Encoding.UTF8.GetString(output.ToArray());
    }
}
