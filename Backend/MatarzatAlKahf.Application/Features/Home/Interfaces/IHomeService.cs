using MatarzatAlKahf.Application.Features.Home.DTOs;

namespace MatarzatAlKahf.Application.Features.Home.Interfaces;

public interface IHomeService
{
    Task<HomeResponse> GetAsync(CancellationToken cancellationToken);
}
