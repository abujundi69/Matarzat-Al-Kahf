using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;

namespace MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
public interface IUpdateSocialLinkService
{
    Task<SocialLinkResponse> UpdateAsync(Guid id, SaveSocialLinkRequest request, CancellationToken cancellationToken);
}
