namespace MatarzatAlKahf.Infrastructure.Persistence.Development;

public interface IDevelopmentDataSeeder
{
    Task<DevelopmentSeedResult> SeedAsync(CancellationToken cancellationToken);
}
