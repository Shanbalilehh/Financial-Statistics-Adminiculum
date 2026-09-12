namespace FinancialStatisticsAdminiculum.Core.Exceptions
{
    // Base for all persistence-layer failures
    public abstract class PersistenceException : SemanticDomainException
    {
        protected PersistenceException(string message)
            : base(message, "PersistenceCommunity")
        {
        }
    }

    // Transient - DB is temporarily unreachable (timeout, deadlock, pool exhausted)
    public class PersistenceUnavailableException : PersistenceException
    {
        public PersistenceUnavailableException(string message)
            : base(message)
        {
        }
    }

    // Constraint - operation violates a data integrity rule (FK, unique, not-null, check)
    public class PersistenceConstraintException : PersistenceException
    {
        public PersistenceConstraintException(string message)
            : base(message)
        {
        }
    }

    public class PersistenceCriticalException : PersistenceException
    {
        public PersistenceCriticalException(string message)
            : base(message)
        {
        }
    }

    // Catch-all - unexpected persistence error with no specific classification yet
    public class PersistenceUnexpectedException : PersistenceException
    {
        public PersistenceUnexpectedException(string message)
            : base(message)
        {
        }
    }
}