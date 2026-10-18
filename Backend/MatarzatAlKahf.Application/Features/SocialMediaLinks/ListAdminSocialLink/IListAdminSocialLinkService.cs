using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;

namespace MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
public interface IListAdminSocialLinkService
{
    Task<IReadOnlyList<SocialLinkResponse>> ListAdminAsync(CancellationToken cancellationToken);
}
