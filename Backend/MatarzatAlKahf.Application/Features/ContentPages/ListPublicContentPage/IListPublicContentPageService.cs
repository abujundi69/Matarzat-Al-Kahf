using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
public interface IListPublicContentPageService
{
    Task<IReadOnlyList<ContentPageResponse>> ListPublicAsync(ContentPageType? type, CancellationToken cancellationToken);
}
