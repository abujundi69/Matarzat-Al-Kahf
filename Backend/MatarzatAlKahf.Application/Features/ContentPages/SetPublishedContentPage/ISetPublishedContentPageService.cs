using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
public interface ISetPublishedContentPageService
{
    Task SetPublishedAsync(Guid id, bool published, CancellationToken cancellationToken);
}
