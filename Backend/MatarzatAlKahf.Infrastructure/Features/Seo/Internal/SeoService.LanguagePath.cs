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
    private static string LanguagePath(LanguageCode language) => language == LanguageCode.English ? "en" : "ar";
}
