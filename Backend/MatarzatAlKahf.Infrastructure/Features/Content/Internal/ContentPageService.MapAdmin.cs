using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;
public sealed partial class ContentPageService
{
    private static AdminContentPageResponse MapAdmin(ContentPage page) => new(page.Id, page.Type, page.IsPublished, page.Translations.OrderBy(item => item.Language).Select(item => new ContentPageTranslationRequest(item.Language, item.Title, item.Slug, item.Content, EmptyToNull(item.MetaTitle), EmptyToNull(item.MetaDescription))).ToArray(), page.CreatedAtUtc, page.UpdatedAtUtc);
}
