using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Persistence.Execution;

internal static class DbUpdateExceptionTranslator
{
    private const int SqlServerDuplicateKey = 2601;
    private const int SqlServerUniqueConstraint = 2627;
    private const int SqliteConstraint = 19;

    public static bool IsUniqueViolation(DbUpdateException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        Exception? current = exception;
        while (current is not null)
        {
            if (current is SqlException sqlException
                && sqlException.Number is SqlServerDuplicateKey or SqlServerUniqueConstraint)
            {
                return true;
            }

            if (string.Equals(
                    current.GetType().FullName,
                    "Microsoft.Data.Sqlite.SqliteException",
                    StringComparison.Ordinal)
                && current.GetType().GetProperty("SqliteErrorCode")?.GetValue(current) is int code
                && code == SqliteConstraint)
            {
                return true;
            }

            current = current.InnerException;
        }

        return false;
    }

}
