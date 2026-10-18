using MatarzatAlKahf.Application.Features.Seo.DTOs;

namespace MatarzatAlKahf.Application.Features.Seo.Interfaces;
public interface IRobotsSeoService
{
    Task<string> RobotsAsync(CancellationToken cancellationToken);
}
