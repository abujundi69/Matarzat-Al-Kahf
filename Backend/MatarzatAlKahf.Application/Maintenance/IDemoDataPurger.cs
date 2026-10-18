namespace MatarzatAlKahf.Application.Maintenance;

public interface IDemoDataPurger
{
    Task<DemoDataPurgeResult> PurgeAsync(CancellationToken cancellationToken);
}
