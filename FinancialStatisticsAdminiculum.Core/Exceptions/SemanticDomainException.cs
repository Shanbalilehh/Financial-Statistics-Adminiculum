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
}
