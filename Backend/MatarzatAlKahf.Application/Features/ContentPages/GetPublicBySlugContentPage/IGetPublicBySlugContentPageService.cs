using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
public interface IGetPublicBySlugContentPageService
{
    Task<ContentPageResponse> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken);
}
