using MatarzatAlKahf.Application.Features.Seo.DTOs;

namespace MatarzatAlKahf.Application.Features.Seo.Interfaces;
public interface IUpdateSeoService
{
    Task<SeoSettingsResponse> UpdateAsync(SaveSeoSettingsRequest request, CancellationToken cancellationToken);
}
