using System.Text;
using System.Xml;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.Seo.DTOs;
using MatarzatAlKahf.Application.Features.Seo.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Seo.Services;
public sealed partial class SeoService
{
    private static void Validate(SaveSeoSettingsRequest request)
    {
        if (!Uri.TryCreate(request.BaseUrl, UriKind.Absolute, out Uri? uri) || uri.Scheme is not ("http" or "https") || request.BaseUrl.Length > 1000)
        {
            throw ValidationException.For(nameof(request.BaseUrl), "A valid base URL is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ArabicTitle) || string.IsNullOrWhiteSpace(request.EnglishTitle) || request.ArabicTitle.Length > 300 || request.EnglishTitle.Length > 300 || request.ArabicDescription.Length > 1000 || request.EnglishDescription.Length > 1000 || request.RobotsAdditionalRules?.Length > 4000)
        {
            throw ValidationException.For(nameof(request), "SEO settings are invalid.");
        }
    }
}
