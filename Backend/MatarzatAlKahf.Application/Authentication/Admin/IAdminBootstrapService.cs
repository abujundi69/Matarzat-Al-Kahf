namespace MatarzatAlKahf.Application.Authentication.Admin;
public interface IAdminBootstrapService
{
    Task<BootstrapAdminResult> BootstrapAsync(BootstrapAdminRequest request, CancellationToken cancellationToken);
}
