using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;

namespace MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
public interface IGetSocialLinkService
{
    Task<SocialLinkResponse> GetAsync(Guid id, CancellationToken cancellationToken);
}
