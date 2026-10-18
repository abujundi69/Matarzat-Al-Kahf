using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;

namespace MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
public interface IListPublicSocialLinkService
{
    Task<IReadOnlyList<SocialLinkResponse>> ListPublicAsync(CancellationToken cancellationToken);
}
