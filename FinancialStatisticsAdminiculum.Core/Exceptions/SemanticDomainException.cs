using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialStatisticsAdminiculum.Core.Exceptions
{
    public abstract class SemanticDomainException : Exception
    {
        public string RiskCommunity { get; }
        protected SemanticDomainException(string message, string riskCommunity) 
            : base(message)
        {
            RiskCommunity = riskCommunity;
        }

    }
    // 2. Concrete implementation for the AI Engine
    public class NlpEngineUnavailableException : SemanticDomainException
    {
        public NlpEngineUnavailableException(string message) 
            : base(message, "NlpCommunity")
        {
        }
    }
    // 3. Concrete implementation for the Database/Market Data
    public class MarketDataUnavailableException : SemanticDomainException
    {
        public MarketDataUnavailableException(string message) 
            : base(message, "PersistenceCommunity")
        {
        }
    }

    public class NlpProcessingException : SemanticDomainException
    {
        public NlpProcessingException(string message) 
            : base(message, "NlpCommunity")
        {
        }
    }
}
