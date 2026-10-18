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
    public async Task<IReadOnlyList<AdminContentPageResponse>> ListAdminAsync(CancellationToken cancellationToken)
    {
        List<ContentPage> pages = await dbContext.ContentPages.AsNoTracking().Include(page => page.Translations).Where(page => !page.IsDeleted).OrderBy(page => page.Type).ToListAsync(cancellationToken);
        return pages.Select(MapAdmin).ToArray();
    }
}
