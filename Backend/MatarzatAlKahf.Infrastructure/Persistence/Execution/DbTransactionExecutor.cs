using System.Data;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Persistence.Execution;

internal static class DbTransactionExecutor
{
    public static async Task<TResult> ExecuteAsync<TResult>(
        MatarzatAlKahfDbContext dbContext,
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(operation);

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                isolationLevel,
                cancellationToken);

            TResult result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }

    public static Task ExecuteAsync(
        MatarzatAlKahfDbContext dbContext,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return ExecuteAsync(
            dbContext,
            async token =>
            {
                await operation(token);
                return true;
            },
            cancellationToken,
            isolationLevel);
    }
}
