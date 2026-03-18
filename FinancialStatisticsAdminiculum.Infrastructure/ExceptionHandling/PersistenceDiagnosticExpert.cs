using Npgsql;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Core.Exceptions;

namespace FinancialStatisticsAdminiculum.Infrastructure.ExceptionHandling
{
    public class PersistenceDiagnosticExpert : IDiagnosticExpert
    {
        public RecoveryDecision Evaluate(Exception technicalException)
        {
            if (technicalException is NpgsqlException pgEx)
            {
                // Transient issues - safe to retry
                if (IsTransient(pgEx))
                {
                    return new RecoveryDecision(
                        DiagnosticAction.Retry,
                        new PersistenceUnavailableException("The database is temporarily unavailable. Please try again.")
                    );
                }

                // Data integrity / corruption - do not retry, terminate to prevent further damage
                if (IsCritical(pgEx))
                {
                    return new RecoveryDecision(
                        DiagnosticAction.TerminateSystem,
                        new PersistenceCriticalException("Fatal error: System Terminated")
                    );
                }

                // Constraint violations - safe to surface to caller, no retry
                if (IsConstraintViolation(pgEx))
                {
                    return new RecoveryDecision(
                        DiagnosticAction.FailSafe,
                        new PersistenceConstraintException("The operation violates a data integrity rule and could not be completed.")
                    );
                }
            }

            // Default fail-safe for unpredictable persistence errors - Future implementation
            return new RecoveryDecision(
                DiagnosticAction.FailSafe,
                new PersistenceUnexpectedException("An unexpected error occurred while accessing the financial data store.")
            );
        }

        // --- Postgres error code classification ---
        // Full reference: https://www.postgresql.org/docs/current/errcodes-appendix.html

        private static bool IsTransient(NpgsqlException ex) =>
            ex.SqlState is
                PostgresErrorCodes.LockNotAvailable        or  // 55P03 - could not obtain lock
                PostgresErrorCodes.DeadlockDetected        or  // 40P01 - deadlock
                PostgresErrorCodes.QueryCanceled           or  // 57014 - statement timeout
                PostgresErrorCodes.TooManyConnections      or  // 53300 - connection pool exhausted
                PostgresErrorCodes.ConnectionFailure;          // 08006 - connection dropped mid-query

        private static bool IsCritical(NpgsqlException ex) =>
            ex.SqlState is
                PostgresErrorCodes.DataCorrupted          or  // XX001 - internal data corruption
                PostgresErrorCodes.IndexCorrupted         or  // XX002 - corrupted index
                PostgresErrorCodes.DiskFull              or  // 53100 - out of disk space
                PostgresErrorCodes.OutOfMemory;               // 53200 - out of memory

        private static bool IsConstraintViolation(NpgsqlException ex) =>
            ex.SqlState is
                PostgresErrorCodes.NotNullViolation       or  // 23502
                PostgresErrorCodes.ForeignKeyViolation    or  // 23503
                PostgresErrorCodes.UniqueViolation        or  // 23505
                PostgresErrorCodes.CheckViolation;            // 23514
    }
}