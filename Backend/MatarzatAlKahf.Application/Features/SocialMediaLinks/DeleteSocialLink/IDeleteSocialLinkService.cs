using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;

namespace MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
public interface IDeleteSocialLinkService
{
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
