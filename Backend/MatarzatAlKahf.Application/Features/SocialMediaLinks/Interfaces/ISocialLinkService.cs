using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;

namespace MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
public interface ISocialLinkService : IListPublicSocialLinkService, IListAdminSocialLinkService, IGetSocialLinkService, ICreateSocialLinkService, IUpdateSocialLinkService, IDeleteSocialLinkService
{
}
