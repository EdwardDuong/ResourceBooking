using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ResourceBooking.Infrastructure.Persistence.Repositories;

/// <summary>
/// Recognizes a unique-index violation raised by SaveChanges without taking a
/// hard dependency on a specific ADO.NET provider. Production runs on SQL
/// Server; the concurrency test suite runs the same code path against
/// SQLite (see ADR-0001) - both need to be recognized here.
/// </summary>
internal static class UniqueConstraintViolationDetector
{
    private const int SqlServerUniqueConstraintViolation = 2627;
    private const int SqlServerUniqueIndexViolation = 2601;

    public static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sqlEx)
        {
            return sqlEx.Number is SqlServerUniqueConstraintViolation or SqlServerUniqueIndexViolation;
        }

        return ex.InnerException?.Message.Contains(
            "UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
