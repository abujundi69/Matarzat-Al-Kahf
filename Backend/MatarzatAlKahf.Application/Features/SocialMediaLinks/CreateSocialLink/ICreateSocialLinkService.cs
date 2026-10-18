using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;

namespace MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
public interface ICreateSocialLinkService
{
    Task<SocialLinkResponse> CreateAsync(SaveSocialLinkRequest request, CancellationToken cancellationToken);
}
