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
    private static void Validate(SaveSocialLinkRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Url) || request.Url.Trim().Length > 2000 || !Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out Uri? uri) || uri.Scheme is not ("http" or "https"))
        {
            throw ValidationException.For(nameof(request.Url), "A valid HTTP(S) URL is required.");
        }
    }
}
