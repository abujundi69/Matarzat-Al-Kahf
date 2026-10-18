using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
public interface IDeleteContentPageService
{
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
