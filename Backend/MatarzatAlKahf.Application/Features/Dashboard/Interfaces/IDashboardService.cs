using MatarzatAlKahf.Application.Features.Dashboard.DTOs;

namespace MatarzatAlKahf.Application.Features.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> GetAsync(CancellationToken cancellationToken);
}
