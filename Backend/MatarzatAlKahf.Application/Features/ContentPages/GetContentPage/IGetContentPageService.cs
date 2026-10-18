using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
public interface IGetContentPageService
{
    Task<AdminContentPageResponse> GetAsync(Guid id, CancellationToken cancellationToken);
}
