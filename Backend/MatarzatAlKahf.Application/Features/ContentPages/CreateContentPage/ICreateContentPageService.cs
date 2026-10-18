using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
public interface ICreateContentPageService
{
    Task<AdminContentPageResponse> CreateAsync(SaveContentPageRequest request, CancellationToken cancellationToken);
}
