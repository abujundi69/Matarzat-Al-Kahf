using MatarzatAlKahf.Application.Features.Seo.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers;
[ApiController]
[Tags("SEO")]
public sealed class SeoController(ISitemapSeoService sitemapSeoService, IRobotsSeoService robotsSeoService) : ControllerBase
{
    [HttpGet("/sitemap.xml")]
    public async Task<ContentResult> Sitemap(CancellationToken cancellationToken) => Content(await sitemapSeoService.SitemapAsync(cancellationToken), "application/xml", System.Text.Encoding.UTF8);
    [HttpGet("/robots.txt")]
    public async Task<ContentResult> Robots(CancellationToken cancellationToken) => Content(await robotsSeoService.RobotsAsync(cancellationToken), "text/plain", System.Text.Encoding.UTF8);
}
