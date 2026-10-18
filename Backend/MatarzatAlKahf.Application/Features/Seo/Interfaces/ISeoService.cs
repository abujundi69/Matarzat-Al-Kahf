using MatarzatAlKahf.Application.Features.Seo.DTOs;

namespace MatarzatAlKahf.Application.Features.Seo.Interfaces;
public interface ISeoService : IGetSeoService, IUpdateSeoService, ISitemapSeoService, IRobotsSeoService
{
}
