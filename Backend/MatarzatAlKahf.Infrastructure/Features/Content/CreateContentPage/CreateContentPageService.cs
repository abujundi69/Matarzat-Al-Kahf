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
    public async Task<AdminContentPageResponse> CreateAsync(SaveContentPageRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var page = new ContentPage();
        Apply(page, request);
        dbContext.ContentPages.Add(page);
        await SaveAsync(cancellationToken);
        await AuditAsync("content.page.created", page, cancellationToken);
        return MapAdmin(page);
    }
}
