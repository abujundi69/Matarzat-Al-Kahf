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
    private Task AuditAsync(string action, SocialMediaLink link, CancellationToken cancellationToken) => auditService.RecordAsync(action, nameof(SocialMediaLink), link.Id, $"Social link {link.Id} was changed.", cancellationToken);
}
