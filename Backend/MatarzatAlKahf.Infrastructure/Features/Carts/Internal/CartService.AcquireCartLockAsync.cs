using System.Data;
using System.Data.Common;
using System.Globalization;
using MatarzatAlKahf.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MatarzatAlKahf.Infrastructure.Features.Carts.Services;

public sealed partial class CartService
{
    private async Task AcquireCartLockAsync(
        string resource,
        CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsSqlServer())
        {
            return;
        }

        var transaction = dbContext.Database.CurrentTransaction
            ?? throw new InvalidOperationException(
                "A cart application lock requires an active database transaction.");
        DbConnection connection = dbContext.Database.GetDbConnection();
        await using DbCommand command = connection.CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandText = """
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock
                @Resource = @resource,
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 15000;
            SELECT @result;
            """;
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = "@resource";
        parameter.DbType = DbType.String;
        parameter.Size = 255;
        parameter.Value = $"matarzat-cart:{resource}";
        command.Parameters.Add(parameter);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null || Convert.ToInt32(result, CultureInfo.InvariantCulture) < 0)
        {
            throw new ConcurrencyConflictException(
                "The cart is being updated by another request. Retry the operation.");
        }
    }
}
