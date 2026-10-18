using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
public interface IListAdminContentPageService
{
    Task<IReadOnlyList<AdminContentPageResponse>> ListAdminAsync(CancellationToken cancellationToken);
}
