using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
public interface IUpdateContentPageService
{
    Task<AdminContentPageResponse> UpdateAsync(Guid id, SaveContentPageRequest request, CancellationToken cancellationToken);
}
