using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;
using MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;
public sealed partial class SocialLinkService
{
    private static void Apply(SocialMediaLink link, SaveSocialLinkRequest request)
    {
        link.Platform = request.Platform;
        link.Url = request.Url.Trim();
        link.IsActive = request.IsActive;
        link.SortOrder = request.SortOrder;
    }
}
