namespace FinancialStatisticsAdminiculum.Core.Exceptions
{
    public abstract class NlpException : SemanticDomainException
    {
        protected NlpException(string message)
            : base(message, "NlpCommunity")
        {
        }
    }

    public class NlpEngineUnavailableException : NlpException
    {
        public NlpEngineUnavailableException(string message) : base(message) { }
    }

    public class NlpProcessingException : NlpException
    {
        public NlpProcessingException(string message) : base(message) { }
    }

    // Mirrors PersistenceUnexpectedException
    public class NlpUnexpectedException : NlpException
    {
        public NlpUnexpectedException(string message) : base(message) { }
    }

    public class NlpCriticalException : NlpException
    {
        public NlpCriticalException(string message) : base(message) { }
    }
}