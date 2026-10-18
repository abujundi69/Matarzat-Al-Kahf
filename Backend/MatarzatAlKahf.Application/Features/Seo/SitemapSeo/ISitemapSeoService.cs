using MatarzatAlKahf.Application.Features.Seo.DTOs;

namespace MatarzatAlKahf.Application.Features.Seo.Interfaces;
public interface ISitemapSeoService
{
    Task<string> SitemapAsync(CancellationToken cancellationToken);
}
