using System.Data;
using MatarzatAlKahf.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Persistence.Development;

/// <summary>
/// Adds the development catalogue and missing store defaults. Existing rows, including
/// soft-deleted rows, are left alone. Accounts are managed by the administrator bootstrap.
/// </summary>
public sealed partial class DevelopmentDataSeeder(MatarzatAlKahfDbContext dbContext)
    : IDevelopmentDataSeeder
{
    private readonly MatarzatAlKahfDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    // EF's migration lock alone does not protect the explicit seed command. Both entry
    // points take this transaction-owned lock before inspecting or inserting seed rows.
    private const string SeedLockSql = """
        DECLARE @result int;
        EXEC @result = sys.sp_getapplock
            @Resource = N'MatarzatAlKahf.DevelopmentSeed',
            @LockMode = N'Exclusive',
            @LockOwner = N'Transaction',
            @LockTimeout = 60000;
        IF @result < 0
            THROW 51000, 'Could not acquire the development seed lock.', 1;
        """;

    public DevelopmentSeedResult Seed() => SeedAsync(CancellationToken.None)
        .GetAwaiter().GetResult();

    public async Task<DevelopmentSeedResult> SeedAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseReadyAsync(cancellationToken);
        return await SeedDatabaseAsync(cancellationToken);
    }

    /// <summary>Entry point for EF's synchronous UseSeeding callback (including PMC).</summary>
    public DevelopmentSeedResult SeedDatabase() =>
        SeedDatabaseAsync(CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    /// Entry point for EF's UseAsyncSeeding callback. Pending migrations must not be
    /// inspected while EF is applying them. An existing EF transaction is respected.
    /// </summary>
    public async Task<DevelopmentSeedResult> SeedDatabaseAsync(
        CancellationToken cancellationToken)
    {
        if (_dbContext.ChangeTracker.HasChanges())
        {
            throw new InvalidOperationException(
                "Run development seeding with a context that has no pending changes.");
        }

        HashSet<object> previouslyTracked = _dbContext.ChangeTracker.Entries()
            .Select(entry => entry.Entity).ToHashSet(ReferenceEqualityComparer.Instance);

        void DetachSeedEntries()
        {
            foreach (var entry in _dbContext.ChangeTracker.Entries().ToArray())
            {
                if (!previouslyTracked.Contains(entry.Entity))
                {
                    entry.State = EntityState.Detached;
                }
            }
        }

        async Task<DevelopmentSeedResult> RunAsync()
        {
            try
            {
                if (_dbContext.Database.IsSqlServer())
                {
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        SeedLockSql, cancellationToken);
                }

                return await SeedCoreAsync(cancellationToken);
            }
            catch
            {
                // Re-read after a transient failure, even if SaveChanges accepted states.
                // The caller's previously tracked entities remain untouched.
                DetachSeedEntries();
                throw;
            }
        }

        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await RunAsync();
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                DevelopmentSeedResult result = await RunAsync();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                DetachSeedEntries();
                throw;
            }
        });
    }

    private async Task<DevelopmentSeedResult> SeedCoreAsync(CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        int locations = await SeedDeliveryLocationsAsync(now, cancellationToken);
        var (categories, products, variants) = await SeedCatalogAsync(now, cancellationToken);
        int pages = await SeedContentAsync(now, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DevelopmentSeedResult(
            0, 0, 0, locations, categories, products, variants, 0, 0, 0, pages);
    }

    private async Task EnsureDatabaseReadyAsync(CancellationToken cancellationToken)
    {
        const string migrationCommand =
            "dotnet ef database update --project MatarzatAlKahf.Infrastructure "
            + "--startup-project MatarzatAlKahf.Api";
        if (!await _dbContext.Database.CanConnectAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                $"The development database is unavailable. Apply migrations with: {migrationCommand}");
        }

        if ((await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            throw new InvalidOperationException(
                $"The development database has pending migrations. Apply them with: {migrationCommand}");
        }
    }

    private static TEntity? ResolveSeedEntity<TEntity>(
        IEnumerable<TEntity> entities,
        Func<TEntity, bool> predicate,
        string naturalKey)
        where TEntity : BaseEntity
    {
        TEntity[] matches = entities.Where(predicate).DistinctBy(entity => entity.Id).ToArray();
        return matches.Length switch
        {
            0 => null,
            1 => matches[0],
            _ => throw new InvalidOperationException(
                $"Development seed key '{naturalKey}' matches multiple {typeof(TEntity).Name} records.")
        };
    }
}
